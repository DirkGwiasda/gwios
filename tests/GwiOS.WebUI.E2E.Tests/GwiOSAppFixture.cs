using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.Playwright;

namespace GwiOS.WebUI.E2E.Tests;

/// <summary>
/// Starts the real GwiOS.WebUI as a subprocess, in the "E2E" hosting environment so it authenticates every request
/// as a fixed test user instead of demanding a real Keycloak sign-in, and launches one Chromium instance shared by
/// all end-to-end tests. Stops both when the test run ends.
/// </summary>
public sealed class GwiOSAppFixture : IAsyncLifetime
{
    private const string TestConnectionStringVariableName = "GwiOS.DB.TestConnectionstring";
    private const string ConnectionStringVariableName = "GwiOS.DB.Connectionstring";
    private const string BaseUrlValue = "http://127.0.0.1:54327";

    private Process? _webUIProcess;
    private IPlaywright? _playwright;

    /// <summary>
    /// The address the running GwiOS.WebUI answers HTTP requests on.
    /// </summary>
    public string BaseUrl
        => BaseUrlValue;

    /// <summary>
    /// The Chromium instance every test opens its own page in.
    /// </summary>
    public IBrowser? Browser { get; private set; }

    /// <summary>
    /// Opens the page at the given path (e.g. <c>/haushaltsbuch/dashboard</c>) and waits until it is interactive.
    /// Until then the page is only prerendered, and clicks on it would get lost.
    /// </summary>
    public async Task OpenAsync(IPage page, string path)
    {
        await page.GotoAsync($"{BaseUrl}{path}");
        await page.Locator(".gwios-app[data-interactive=true]").WaitForAsync();
    }

    public async ValueTask InitializeAsync()
    {
        StartWebUIProcess();
        await WaitUntilReadyAsync();

        _playwright = await Playwright.CreateAsync();
        Browser = await _playwright.Chromium.LaunchAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.CloseAsync();
        }

        _playwright?.Dispose();
        StopWebUIProcess();
    }

    private void StartWebUIProcess()
    {
        ProcessStartInfo startInfo = new("dotnet")
        {
            WorkingDirectory = GetRepositoryRoot(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(GetWebUIProjectPath());
        startInfo.ArgumentList.Add("--artifacts-path");
        startInfo.ArgumentList.Add(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "build",
            "gwios-dotnet"));
        startInfo.ArgumentList.Add("--no-launch-profile");
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "E2E";
        startInfo.Environment["ASPNETCORE_URLS"] = BaseUrl;
        startInfo.Environment[ConnectionStringVariableName] = ReadTestConnectionString();

        _webUIProcess = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start the GwiOS.WebUI process.");
    }

    private void StopWebUIProcess()
    {
        if (_webUIProcess is null)
        {
            return;
        }

        if (!_webUIProcess.HasExited)
        {
            _webUIProcess.Kill(entireProcessTree: true);
            _webUIProcess.WaitForExit();
        }

        _webUIProcess.Dispose();
    }

    private async Task WaitUntilReadyAsync()
    {
        using HttpClient client = new();
        for (int attempt = 0; attempt < 120; attempt++)
        {
            if (_webUIProcess!.HasExited)
            {
                throw new InvalidOperationException(
                    $"GwiOS.WebUI exited early with code {_webUIProcess.ExitCode} while starting up for the "
                    + "end-to-end tests.");
            }

            try
            {
                HttpResponseMessage response = await client.GetAsync(BaseUrl);
                if (response.StatusCode != HttpStatusCode.ServiceUnavailable)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // Not listening yet.
            }

            await Task.Delay(500);
        }

        throw new InvalidOperationException("GwiOS.WebUI did not become ready in time for the end-to-end tests.");
    }

    // Baked in at compile time, so it resolves the repository regardless of where the test output ends up.
    private static string GetRepositoryRoot([CallerFilePath] string sourceFilePath = "")
    {
        string? directory = Path.GetDirectoryName(sourceFilePath);
        while ((directory is not null) && !File.Exists(Path.Combine(directory, "CLAUDE.md")))
        {
            directory = Path.GetDirectoryName(directory);
        }

        return directory
            ?? throw new InvalidOperationException("Could not locate the GwiOS repository root from the test source path.");
    }

    private static string GetWebUIProjectPath()
        => Path.Combine(GetRepositoryRoot(), "src", "GwiOS.WebUI", "GwiOS.WebUI.csproj");

    private static string ReadTestConnectionString()
        => Environment.GetEnvironmentVariable(TestConnectionStringVariableName)
            ?? throw new InvalidOperationException(
                $"The environment variable '{TestConnectionStringVariableName}' must be set. It must contain the "
                + "Npgsql connection string of the PostgreSQL test database the end-to-end tests run GwiOS.WebUI "
                + "against.");
}
