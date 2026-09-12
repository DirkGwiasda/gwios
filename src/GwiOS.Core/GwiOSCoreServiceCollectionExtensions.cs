using GwiOS.Core.CrossCutting.Logging;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Managers;
using GwiOS.Core.CrossCutting.Logging.Infrastructure;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.CrossCutting.Persons.Infrastructure;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts;
using GwiOS.Core.ToDos.Infrastructure;
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
    /// <see cref="IPersonRepository.EnsureStorageCreatedAsync"/> and
    /// <see cref="IToDoRepository.EnsureStorageCreatedAsync"/>.
    /// </param>
    /// <returns>The same container, for chaining.</returns>
    public static IServiceCollection AddGwiOSCore(this IServiceCollection services, string connectionString)
    {
        services.AddNpgsqlDataSource(connectionString);
        services.TryAddSingleton(TimeProvider.System);
        AddLogging(services);
        AddPersons(services);
        AddToDos(services);
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
}
