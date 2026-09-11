using GwiOS.Core.CrossCutting.Logging;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Managers;
using GwiOS.Core.CrossCutting.Logging.Infrastructure;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.CrossCutting.Persons.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace GwiOS.Core.Tests.GwiOSCoreServiceCollectionExtensions;

/// <summary>
/// Covers <c>GwiOSCoreServiceCollectionExtensions.AddGwiOSCore</c>. No database connection is opened: registrations
/// are only resolved, and the container validates on build that every registration can be constructed.
/// </summary>
public sealed class AddGwiOSCoreTests : IDisposable
{
    private const string ConnectionString = "Host=localhost;Database=gwios_unit_test;Username=gwios_user";

    private readonly ServiceProvider _serviceProvider = CreateServiceProvider();
    private readonly IServiceScope _scope;

    public AddGwiOSCoreTests()
    {
        _scope = _serviceProvider.CreateScope();
    }

    [Fact]
    public void RegistersThePostgresRepositoryAsLogEntryRepository()
    {
        ILogEntryRepository logEntryRepository = _scope.ServiceProvider.GetRequiredService<ILogEntryRepository>();

        Assert.IsType<LogEntryPostgresRepository>(logEntryRepository);
    }

    [Fact]
    public void RegistersTheLogEntryManager()
    {
        ILogEntryManager logEntryManager = _scope.ServiceProvider.GetRequiredService<ILogEntryManager>();

        Assert.IsType<LogEntryManager>(logEntryManager);
    }

    [Fact]
    public void RegistersTheDefaultLoggerForAnyType()
    {
        ILogger<AddGwiOSCoreTests> logger = _scope.ServiceProvider.GetRequiredService<ILogger<AddGwiOSCoreTests>>();

        Assert.IsType<DefaultLogger<AddGwiOSCoreTests>>(logger);
    }

    [Fact]
    public void RegistersThePostgresRepositoryAsPersonRepository()
    {
        IPersonRepository personRepository = _scope.ServiceProvider.GetRequiredService<IPersonRepository>();

        Assert.IsType<PersonPostgresRepository>(personRepository);
    }

    [Fact]
    public void RegistersThePersonValidator()
    {
        IPersonValidator personValidator = _scope.ServiceProvider.GetRequiredService<IPersonValidator>();

        Assert.IsType<PersonValidator>(personValidator);
    }

    [Fact]
    public void RegistersThePersonManager()
    {
        IPersonManager personManager = _scope.ServiceProvider.GetRequiredService<IPersonManager>();

        Assert.IsType<PersonManager>(personManager);
    }

    [Fact]
    public void RegistersADataSourceForTheDatabaseOfTheConnectionString()
    {
        NpgsqlDataSource dataSource = _serviceProvider.GetRequiredService<NpgsqlDataSource>();

        Assert.Equal("gwios_unit_test", new NpgsqlConnectionStringBuilder(dataSource.ConnectionString).Database);
    }

    public void Dispose()
    {
        _scope.Dispose();
        _serviceProvider.Dispose();
    }

    private static ServiceProvider CreateServiceProvider()
    {
        ServiceCollection services = new();
        services.AddGwiOSCore(ConnectionString);
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }
}
