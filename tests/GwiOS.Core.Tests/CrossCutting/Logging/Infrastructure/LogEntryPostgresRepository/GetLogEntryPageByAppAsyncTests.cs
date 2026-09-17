using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.CrossCutting.Logging.Infrastructure.LogEntryPostgresRepository;

/// <summary>
/// Covers <c>LogEntryPostgresRepository.GetLogEntryPageByAppAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class GetLogEntryPageByAppAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly PostgresTestDatabase _database = database;
    private readonly string _appName = $"test-{Guid.NewGuid():N}";
    private readonly string _otherAppName = $"test-{Guid.NewGuid():N}";
    private readonly DateTimeOffset _timestamp = new(2026, 9, 12, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ReturnsOnlyTheLogEntriesOfTheGivenApp()
    {
        LogEntry logEntry = new(_appName, "Tests", "Message", LogLevel.Information);
        await _database.LogEntryRepository.InsertAsync(logEntry);
        await _database.LogEntryRepository.InsertAsync(
            new LogEntry(_otherAppName, "Tests", "Message", LogLevel.Information));

        LogEntryPage page = await _database.LogEntryRepository.GetLogEntryPageByAppAsync(_appName, 1, 10);

        Assert.Equal(logEntry.Id, Assert.Single(page.Entries).Id);
        Assert.Equal(1, page.TotalCount);
    }

    [Fact]
    public async Task ReturnsTheLogEntriesNewestFirst()
    {
        List<LogEntry> logEntries = await InsertLogEntriesAsync(2);

        LogEntryPage page = await _database.LogEntryRepository.GetLogEntryPageByAppAsync(_appName, 1, 10);

        Guid[] expectedIds = [logEntries[1].Id, logEntries[0].Id];
        Assert.Equal(expectedIds, page.Entries.Select(logEntry => logEntry.Id));
    }

    [Fact]
    public async Task ReturnsTheLogEntriesOfTheRequestedPageAndTheNumberOfAllLogEntries()
    {
        List<LogEntry> logEntries = await InsertLogEntriesAsync(5);

        LogEntryPage page = await _database.LogEntryRepository.GetLogEntryPageByAppAsync(_appName, 2, 2);

        Guid[] expectedIds = [logEntries[2].Id, logEntries[1].Id];
        Assert.Equal(expectedIds, page.Entries.Select(logEntry => logEntry.Id));
        Assert.Equal(5, page.TotalCount);
    }

    [Fact]
    public async Task ReturnsNoLogEntriesButTheNumberOfAllLogEntries_WhenThePageLiesBeyondTheLastLogEntry()
    {
        await InsertLogEntriesAsync(3);

        LogEntryPage page = await _database.LogEntryRepository.GetLogEntryPageByAppAsync(_appName, 3, 2);

        Assert.Empty(page.Entries);
        Assert.Equal(3, page.TotalCount);
    }

    [Fact]
    public async Task ReturnsAnEmptyPage_WhenTheAppHasNoLogEntries()
    {
        LogEntryPage page = await _database.LogEntryRepository.GetLogEntryPageByAppAsync(_appName, 1, 10);

        Assert.Empty(page.Entries);
        Assert.Equal(0, page.TotalCount);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _database.LogEntryRepository.DeleteByAppNameAsync(_appName);
        await _database.LogEntryRepository.DeleteByAppNameAsync(_otherAppName);
    }

    // Inserts entries one minute apart, oldest first, and returns them in that order.
    private async Task<List<LogEntry>> InsertLogEntriesAsync(int count)
    {
        List<LogEntry> logEntries = [];
        for (int index = 0; index < count; index++)
        {
            LogEntry logEntry = new(_appName, "Tests", $"Message {index}", LogLevel.Information)
            {
                Timestamp = _timestamp.AddMinutes(index)
            };
            await _database.LogEntryRepository.InsertAsync(logEntry);
            logEntries.Add(logEntry);
        }

        return logEntries;
    }
}
