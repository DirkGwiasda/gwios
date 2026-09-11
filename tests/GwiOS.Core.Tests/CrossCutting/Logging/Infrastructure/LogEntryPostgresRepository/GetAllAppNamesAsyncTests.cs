using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.CrossCutting.Logging.Infrastructure.LogEntryPostgresRepository;

/// <summary>
/// Covers <c>LogEntryPostgresRepository.GetAllAppNamesAsync</c> against the PostgreSQL test database. The database
/// may contain entries of other apps, so the tests only look at the app names they created themselves.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class GetAllAppNamesAsyncTests : IAsyncLifetime
{
    private readonly PostgresTestDatabase _database;
    private readonly string _firstAppName;
    private readonly string _secondAppName;

    public GetAllAppNamesAsyncTests(PostgresTestDatabase database)
    {
        _database = database;
        string appNamePrefix = $"test-{Guid.NewGuid():N}";
        _firstAppName = $"{appNamePrefix}-a";
        _secondAppName = $"{appNamePrefix}-b";
    }

    [Fact]
    public async Task ReturnsEachAppNameOnce()
    {
        await InsertLogEntryAsync(_firstAppName);
        await InsertLogEntryAsync(_firstAppName);
        await InsertLogEntryAsync(_secondAppName);

        List<string> appNames = await _database.LogEntryRepository.GetAllAppNamesAsync();

        Assert.Single(appNames, appName => appName == _firstAppName);
        Assert.Single(appNames, appName => appName == _secondAppName);
    }

    [Fact]
    public async Task ReturnsTheAppNamesInAscendingOrder()
    {
        await InsertLogEntryAsync(_secondAppName);
        await InsertLogEntryAsync(_firstAppName);

        List<string> appNames = await _database.LogEntryRepository.GetAllAppNamesAsync();

        Assert.True(appNames.IndexOf(_firstAppName) < appNames.IndexOf(_secondAppName));
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _database.LogEntryRepository.DeleteByAppNameAsync(_firstAppName);
        await _database.LogEntryRepository.DeleteByAppNameAsync(_secondAppName);
    }

    private async Task InsertLogEntryAsync(string appName)
        => await _database.LogEntryRepository.InsertAsync(
            new LogEntry(appName, "Tests", "Message", LogLevel.Information));
}
