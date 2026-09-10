using GwiOS.Core.CrossCutting.Logging;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Managers;
using GwiOS.Core.CrossCutting.Logging.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace GwiOS.Core;

/// <summary>
/// Registers the services of GwiOS.Core with the dependency injection container.
/// </summary>
public static class GwiOSCoreServiceCollectionExtensions
{
    // Every PostgreSQL server ships this database; it is reachable while the GwiOS database does not exist yet.
    private const string MaintenanceDatabaseName = "postgres";

    /// <summary>
    /// Registers all services of GwiOS.Core, backed by the PostgreSQL database named in the connection string.
    /// </summary>
    /// <param name="services">The container to register the services with.</param>
    /// <param name="connectionString">
    /// Npgsql connection string of the GwiOS database. It must specify the database (<c>Database=...</c>); the
    /// database itself does not have to exist yet, see <see cref="ILogEntryRepository.EnsureStorageCreatedAsync"/>.
    /// </param>
    /// <returns>The same container, for chaining.</returns>
    public static IServiceCollection AddGwiOSCore(this IServiceCollection services, string connectionString)
    {
        AddPostgresDataSources(services, connectionString);
        AddLogging(services);
        return services;
    }

    private static void AddPostgresDataSources(IServiceCollection services, string connectionString)
    {
        services.AddNpgsqlDataSource(connectionString);
        services.AddNpgsqlDataSource(
            connectionString,
            ConfigureMaintenanceDataSource,
            serviceKey: LogEntryPostgresRepository.MaintenanceDataSourceKey);
    }

    private static void ConfigureMaintenanceDataSource(NpgsqlDataSourceBuilder dataSourceBuilder)
    {
        dataSourceBuilder.ConnectionStringBuilder.Database = MaintenanceDatabaseName;
        // The maintenance database is only used once at startup; pooled connections would just sit idle.
        dataSourceBuilder.ConnectionStringBuilder.Pooling = false;
    }

    private static void AddLogging(IServiceCollection services)
    {
        services.AddSingleton(typeof(ILogger<>), typeof(DefaultLogger<>));
        services.AddScoped<ILogEntryRepository, LogEntryPostgresRepository>();
        services.AddScoped<ILogEntryManager, LogEntryManager>();
    }
}
