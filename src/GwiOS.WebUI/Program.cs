using GwiOS.Core;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.WebUI.Components;

namespace GwiOS.WebUI;

public class Program
{
    private const string GwiOSConnectionStringVariableName = "GwiOS.DB.Connectionstring";

    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddGwiOSCore(GetGwiOSConnectionString(builder.Configuration));

        WebApplication app = builder.Build();

        await EnsureStorageCreatedAsync(app.Services);

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        await app.RunAsync();
    }

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
    }
}
