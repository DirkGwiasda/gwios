using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace GwiOS.WebUI.E2E.Tests.Haushaltsbuch;

/// <summary>
/// End-to-end coverage of the Berichte tab of the Haushaltsbuch, driven through a real browser against the real
/// GwiOS.WebUI.
/// </summary>
[Collection(GwiOSAppCollection.Name)]
public sealed class BerichtePageTests(GwiOSAppFixture app) : IAsyncLifetime
{
    private IBrowserContext _context = default!;
    private IPage _page = default!;

    public async ValueTask InitializeAsync()
    {
        _context = await app.Browser!.NewContextAsync();
        _page = await _context.NewPageAsync();
    }

    public async ValueTask DisposeAsync()
        => await _context.DisposeAsync();

    [Fact]
    public async Task ShowsTheReportSections()
    {
        await _page.GotoAsync($"{app.BaseUrl}/haushaltsbuch/berichte");

        await Expect(_page.Locator("h1")).ToHaveTextAsync("Haushaltsbuch");
        await Expect(_page.GetByText("Soll / Ist")).ToBeVisibleAsync();
        await Expect(_page.GetByText("Ausgaben nach Kategorie")).ToBeVisibleAsync();
        await Expect(_page.GetByText("Monatsverlauf")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task SwitchesTheSelectedRange()
    {
        await _page.GotoAsync($"{app.BaseUrl}/haushaltsbuch/berichte");

        await _page.GetByRole(AriaRole.Button, new() { Name = "Letzte 6 Monate" }).ClickAsync();

        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "Letzte 6 Monate" }))
            .ToHaveAttributeAsync("aria-pressed", "true");
    }
}
