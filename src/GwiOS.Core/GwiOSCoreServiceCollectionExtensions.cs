using GwiOS.Core.CrossCutting.Logging;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Managers;
using GwiOS.Core.CrossCutting.Logging.Infrastructure;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.CrossCutting.Persons.Infrastructure;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.MeasurementPolling;
using GwiOS.Core.Satellites.Domain.Workflows.MeasurementPolling.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteAdoption;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteAdoption.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteBlocking;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteBlocking.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.SatellitePing;
using GwiOS.Core.Satellites.Domain.Workflows.SatellitePing.Contracts;
using GwiOS.Core.Satellites.Infrastructure;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts;
using GwiOS.Core.ToDos.Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GwiOS.Core;

/// <summary>
/// Registers the services of GwiOS.Core with the dependency injection container.
/// </summary>
public static class GwiOSCoreServiceCollectionExtensions
{
    /// <summary>
    /// Registers all services of GwiOS.Core, backed by the PostgreSQL database named in the connection string.
    /// </summary>
    /// <param name="services">The container to register the services with.</param>
    /// <param name="connectionString">
    /// Npgsql connection string of the GwiOS database. The database must already exist; its tables are created by
    /// <see cref="ILogEntryRepository.EnsureStorageCreatedAsync"/>,
    /// <see cref="IPersonRepository.EnsureStorageCreatedAsync"/>,
    /// <see cref="IToDoRepository.EnsureStorageCreatedAsync"/> and
    /// <see cref="ISatelliteRepository.EnsureStorageCreatedAsync"/>.
    /// </param>
    /// <returns>The same container, for chaining.</returns>
    public static IServiceCollection AddGwiOSCore(this IServiceCollection services, string connectionString)
    {
        services.AddNpgsqlDataSource(connectionString);
        services.TryAddSingleton(TimeProvider.System);
        AddLogging(services);
        AddPersons(services);
        AddToDos(services);
        AddSatellites(services);
        return services;
    }

    private static void AddLogging(IServiceCollection services)
    {
        services.AddSingleton(typeof(ILogger<>), typeof(DefaultLogger<>));
        services.AddScoped<ILogEntryRepository, LogEntryPostgresRepository>();
        services.AddScoped<ILogEntryManager, LogEntryManager>();
    }

    private static void AddPersons(IServiceCollection services)
    {
        services.AddScoped<IPersonRepository, PersonPostgresRepository>();
        services.AddScoped<IPersonValidator, PersonValidator>();
        services.AddScoped<IPersonManager, PersonManager>();
    }

    private static void AddToDos(IServiceCollection services)
    {
        services.AddScoped<IToDoRepository, ToDoPostgresRepository>();
        services.AddScoped<IToDoValidator, ToDoValidator>();
        services.AddScoped<IToDoManager, ToDoManager>();
    }

    private static void AddSatellites(IServiceCollection services)
    {
        // Satellite tokens are encrypted with ASP.NET Data Protection; a web host adds its own configuration on top.
        services.AddDataProtection();
        services.AddHttpClient(SatelliteMeasurementHttpClient.HttpClientName, ConfigureSatelliteHttpClient);

        services.AddScoped<ISatelliteRepository, SatellitePostgresRepository>();
        services.AddScoped<ISatelliteValidator, SatelliteValidator>();
        services.AddScoped<ISatelliteTokenGenerator, SatelliteTokenGenerator>();
        services.AddScoped<ISatelliteTokenProtector, SatelliteTokenProtector>();
        services.AddScoped<ISatelliteManager, SatelliteManager>();

        // The in-memory repositories keep their state for the lifetime of the application.
        services.AddSingleton<ISatellitePresenceRepository, SatellitePresenceInMemoryRepository>();
        services.AddScoped<ISatellitePresenceValidator, SatellitePresenceValidator>();
        services.AddScoped<ISatellitePresenceManager, SatellitePresenceManager>();

        services.AddSingleton<ISatelliteCandidateRepository, SatelliteCandidateInMemoryRepository>();
        services.AddScoped<ISatelliteCandidateValidator, SatelliteCandidateValidator>();
        services.AddScoped<ISatelliteCandidateManager, SatelliteCandidateManager>();

        services.AddSingleton<IMeasurementHistoryRepository, MeasurementHistoryInMemoryRepository>();
        services.AddScoped<ISatelliteMeasurementClient, SatelliteMeasurementHttpClient>();
        services.AddScoped<IMeasurementReadingValidator, MeasurementReadingValidator>();
        services.AddScoped<IMeasurementManager, MeasurementManager>();

        services.AddScoped<ISatellitePingWorkflow, SatellitePingWorkflow>();
        services.AddScoped<IMeasurementPollingWorkflow, MeasurementPollingWorkflow>();
        services.AddScoped<ISatelliteOverviewWorkflow, SatelliteOverviewWorkflow>();
        services.AddScoped<ISatelliteAdoptionWorkflow, SatelliteAdoptionWorkflow>();
        services.AddScoped<ISatelliteBlockingWorkflow, SatelliteBlockingWorkflow>();
    }

    // Satellites answer within milliseconds in the LAN; a satellite that does not must not hold up the others for long.
    private static void ConfigureSatelliteHttpClient(HttpClient httpClient)
    {
        httpClient.Timeout = TimeSpan.FromSeconds(5);
        httpClient.MaxResponseContentBufferSize = 64 * 1024;
    }
}
