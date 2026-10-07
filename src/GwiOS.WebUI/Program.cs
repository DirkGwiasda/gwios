using GwiOS.Core;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts;
using GwiOS.WebUI.Api.Backup;
using GwiOS.WebUI.Api.Backup.Contracts;
using GwiOS.WebUI.Api.Satellites;
using GwiOS.WebUI.Api.Satellites.Contracts;
using GwiOS.WebUI.BackgroundServices;
using GwiOS.WebUI.Components;
using GwiOS.WebUI.E2E;
using GwiOS.WebUI.StatusMessages;
using GwiOS.WebUI.StatusMessages.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace GwiOS.WebUI;

public class Program
{
    private const string GwiOSConnectionStringVariableName = "GwiOS.DB.Connectionstring";
    private const string ApiPathPrefix = "/api";

    // A dedicated hosting environment name, set only by the E2E test harness, that swaps Keycloak sign-in for
    // GwiOS.WebUI.E2E.E2ETestAuthenticationHandler; never "Development" or "Production".
    private const string E2EEnvironmentName = "E2E";

    // A ping body is well below 1 KB; larger requests are rejected before they are read.
    private const long MaxSatellitePingBodyBytes = 4096;

    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddGwiOSCore(GetGwiOSConnectionString(builder.Configuration));
        // Scoped, so every circuit and thus every user session has status messages of its own.
        builder.Services.AddScoped<IStatusMessageService, StatusMessageService>();
        builder.Services.AddScoped<ISatellitePingEndpoint, SatellitePingEndpoint>();
        builder.Services.AddScoped<IDatabaseBackupEndpoint, DatabaseBackupEndpoint>();
        builder.Services.Configure<MeasurementPollingOptions>(
            builder.Configuration.GetSection(MeasurementPollingOptions.SectionName));
        builder.Services.AddHostedService<MeasurementPollingService>();

        if (builder.Environment.IsEnvironment(E2EEnvironmentName))
        {
            // "dotnet run" serves the static web assets (the Blazor script, the bundled component styles) only in
            // Development by itself. Without them the end-to-end tests would get pages that never become interactive.
            builder.WebHost.UseStaticWebAssets();
            builder.Services.AddAuthentication(E2ETestAuthenticationHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, E2ETestAuthenticationHandler>(
                    E2ETestAuthenticationHandler.SchemeName,
                    _ => { });
        }
        else
        {
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = builder.Configuration["Keycloak:Authority"];
                    options.ClientId = builder.Configuration["Keycloak:ClientId"];
                    options.ClientSecret = builder.Configuration["Keycloak:ClientSecret"];

                    options.ResponseType = "code";
                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.RequireHttpsMetadata = true; // In Produktion auf 'true' stellen!

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "preferred_username", // Keycloak standardmäßig
                        RoleClaimType = "roles"
                    };
                });
        }

        builder.Services.AddAuthorization();
        builder.Services.AddCascadingAuthenticationState();

        WebApplication app = builder.Build();

        await EnsureStorageCreatedAsync(app.Services);

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        // The API answers with status codes for machines; the not-found page would demand a sign-in and redirect.
        app.UseWhen(
            context => !context.Request.Path.StartsWithSegments(ApiPathPrefix),
            branch => branch.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true));
        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/login", () => Results.Challenge(new Microsoft.AspNetCore.Authentication.AuthenticationProperties { RedirectUri = "/" }, [OpenIdConnectDefaults.AuthenticationScheme]));
        app.MapGet("/logout", () => Results.SignOut(new Microsoft.AspNetCore.Authentication.AuthenticationProperties { RedirectUri = "/" }, [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]));

        MapSatelliteApi(app);
        MapBackupApi(app);
        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        await app.RunAsync();
    }

    // Satellites authenticate with their token, not with the sign-in of users, so the API allows anonymous requests
    // and checks the token itself.
    private static void MapSatelliteApi(WebApplication app)
        => app.MapPost(
                $"{ApiPathPrefix}/satellites/{{name}}/ping",
                (string name, HttpRequest request, ISatellitePingEndpoint endpoint, CancellationToken cancellationToken)
                    => endpoint.HandleAsync(name, request, cancellationToken))
            .AllowAnonymous()
            .DisableAntiforgery()
            .WithMetadata(new RequestSizeLimitAttribute(MaxSatellitePingBodyBytes));

    // Unlike the satellite API, the backup is for signed-in users: an anonymous request is sent to the sign-in.
    private static void MapBackupApi(WebApplication app)
        => app.MapGet(
                $"{ApiPathPrefix}/backup",
                (IDatabaseBackupEndpoint endpoint, CancellationToken cancellationToken)
                    => endpoint.HandleAsync(cancellationToken))
            .RequireAuthorization();

    // Environment variables are part of the configuration; their names are used as keys unchanged.
    private static string GetGwiOSConnectionString(IConfiguration configuration)
        => configuration[GwiOSConnectionStringVariableName]
            ?? throw new InvalidOperationException(
                $"The environment variable '{GwiOSConnectionStringVariableName}' is not set. "
                + "It must contain the Npgsql connection string of the GwiOS database.");

    private static async Task EnsureStorageCreatedAsync(IServiceProvider services)
    {
        // The repositories are scoped, so they must not be resolved from the root provider.
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        ILogEntryRepository logEntryRepository = scope.ServiceProvider.GetRequiredService<ILogEntryRepository>();
        await logEntryRepository.EnsureStorageCreatedAsync();
        IPersonRepository personRepository = scope.ServiceProvider.GetRequiredService<IPersonRepository>();
        await personRepository.EnsureStorageCreatedAsync();
        IToDoRepository toDoRepository = scope.ServiceProvider.GetRequiredService<IToDoRepository>();
        await toDoRepository.EnsureStorageCreatedAsync();
        ISatelliteRepository satelliteRepository = scope.ServiceProvider.GetRequiredService<ISatelliteRepository>();
        await satelliteRepository.EnsureStorageCreatedAsync();
        ICategoryRepository categoryRepository = scope.ServiceProvider.GetRequiredService<ICategoryRepository>();
        await categoryRepository.EnsureStorageCreatedAsync();
        IRecurringItemRepository recurringItemRepository =
            scope.ServiceProvider.GetRequiredService<IRecurringItemRepository>();
        await recurringItemRepository.EnsureStorageCreatedAsync();
        IBookingRepository bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        await bookingRepository.EnsureStorageCreatedAsync();
        IMonthReviewRepository monthReviewRepository =
            scope.ServiceProvider.GetRequiredService<IMonthReviewRepository>();
        await monthReviewRepository.EnsureStorageCreatedAsync();
    }
}
