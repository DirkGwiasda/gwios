using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.CrossCutting.Logging.Infrastructure.LogEntryPostgresRepository;

/// <summary>
/// Covers <c>LogEntryPostgresRepository.GetAllLogEntriesByAppAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class GetAllLogEntriesByAppAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly PostgresTestDatabase _database = database;
    private readonly string _appName = $"test-{Guid.NewGuid():N}";
    private readonly string _otherAppName = $"test-{Guid.NewGuid():N}";

    [Fact]
    public async Task ReturnsOnlyTheLogEntriesOfTheGivenApp()
    {
        LogEntry logEntry = new(_appName, "Tests", "Message", LogLevel.Information);
        await _database.LogEntryRepository.InsertAsync(logEntry);
        await _database.LogEntryRepository.InsertAsync(
            new LogEntry(_otherAppName, "Tests", "Message", LogLevel.Information));

        List<LogEntry> logEntries = await _database.LogEntryRepository.GetAllLogEntriesByAppAsync(_appName);

        Assert.Equal(logEntry.Id, Assert.Single(logEntries).Id);
    }

    [Fact]
    public async Task ReturnsTheLogEntriesNewestFirst()
    {
        LogEntry olderLogEntry = new(_appName, "Tests", "Older", LogLevel.Information)
        {
            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-1)
        };
        LogEntry newerLogEntry = new(_appName, "Tests", "Newer", LogLevel.Information);
        await _database.LogEntryRepository.InsertAsync(olderLogEntry);
        await _database.LogEntryRepository.InsertAsync(newerLogEntry);

        List<LogEntry> logEntries = await _database.LogEntryRepository.GetAllLogEntriesByAppAsync(_appName);

        Guid[] expectedIds = [newerLogEntry.Id, olderLogEntry.Id];
        Assert.Equal(expectedIds, logEntries.Select(logEntry => logEntry.Id));
    }

    [Fact]
    public async Task ReturnsAnEmptyList_WhenTheAppHasNoLogEntries()
    {
        List<LogEntry> logEntries = await _database.LogEntryRepository.GetAllLogEntriesByAppAsync(_appName);

        Assert.Empty(logEntries);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _database.LogEntryRepository.DeleteByAppNameAsync(_appName);
        await _database.LogEntryRepository.DeleteByAppNameAsync(_otherAppName);
    }
}
