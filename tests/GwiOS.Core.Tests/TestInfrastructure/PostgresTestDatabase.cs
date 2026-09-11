using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.CrossCutting.Persons.Infrastructure;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace GwiOS.Core.Tests.TestInfrastructure;

/// <summary>
/// Connects the repositories to the PostgreSQL test database named by the environment variable
/// <c>GwiOS.DB.TestConnectionstring</c> (read from the test process, and on Windows also from the user account),
/// wired as in production, and creates their storage once for all tests sharing this fixture. Only the logger is
/// replaced by a <see cref="LoggerFake{T}"/>, so the repositories under test leave no log entries in the database.
/// Fails with an explanatory message if the variable is not set.
/// </summary>
public sealed class PostgresTestDatabase : IAsyncLifetime
{
    private const string ConnectionStringVariableName = "GwiOS.DB.TestConnectionstring";

    private readonly ServiceProvider _serviceProvider;
    private readonly AsyncServiceScope _scope;

    public PostgresTestDatabase()
    {
        _serviceProvider = CreateServiceProvider(ReadConnectionString());
        _scope = _serviceProvider.CreateAsyncScope();
        LogEntryRepository = _scope.ServiceProvider.GetRequiredService<ILogEntryRepository>();
        PersonRepository = _scope.ServiceProvider.GetRequiredService<IPersonRepository>();
        PersonRepositoryLogger = (LoggerFake<PersonPostgresRepository>)_serviceProvider
            .GetRequiredService<ILogger<PersonPostgresRepository>>();
    }

    /// <summary>
    /// The log entry repository under test, connected to the test database.
    /// </summary>
    public ILogEntryRepository LogEntryRepository { get; }

    /// <summary>
    /// The person repository under test, connected to the test database.
    /// </summary>
    public IPersonRepository PersonRepository { get; }

    /// <summary>
    /// The logger of the person repository. It keeps the entries of all tests sharing this fixture, so tests only
    /// look at the entries of the persons they created themselves.
    /// </summary>
    internal LoggerFake<PersonPostgresRepository> PersonRepositoryLogger { get; }

    public async ValueTask InitializeAsync()
    {
        await LogEntryRepository.EnsureStorageCreatedAsync();
        await PersonRepository.EnsureStorageCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _scope.DisposeAsync();
        await _serviceProvider.DisposeAsync();
    }

    // A process only sees the environment variables that existed when it was started. On Windows the variables of
    // the user account are therefore also read directly, so a newly set variable works without restarting the IDE.
    // On other platforms the user account lookup always returns null.
    private static string ReadConnectionString()
        => Environment.GetEnvironmentVariable(ConnectionStringVariableName)
            ?? Environment.GetEnvironmentVariable(ConnectionStringVariableName, EnvironmentVariableTarget.User)
            ?? throw new InvalidOperationException(
                $"The environment variable '{ConnectionStringVariableName}' is set neither for the test process nor "
                + "for the user account. It must contain the Npgsql connection string of the PostgreSQL test database.");

    private static ServiceProvider CreateServiceProvider(string connectionString)
    {
        ServiceCollection services = new();
        services.AddGwiOSCore(connectionString);
        // Registered last, so it replaces the production logger.
        services.AddSingleton(typeof(ILogger<>), typeof(LoggerFake<>));
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
    }
}
