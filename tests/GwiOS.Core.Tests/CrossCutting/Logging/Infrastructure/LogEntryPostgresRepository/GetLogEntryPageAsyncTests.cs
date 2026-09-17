using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.CrossCutting.Logging.Infrastructure.LogEntryPostgresRepository;

/// <summary>
/// Covers <c>LogEntryPostgresRepository.GetLogEntryPageAsync</c> against the PostgreSQL test database. The database
/// may hold entries of other tests or runs, so the entries written here lie in the far future to come first, and the
/// number of entries is compared with the number before the test.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class GetLogEntryPageAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly PostgresTestDatabase _database = database;
    private readonly string _appName = $"test-{Guid.NewGuid():N}";
    private readonly string _otherAppName = $"test-{Guid.NewGuid():N}";
    private readonly DateTimeOffset _farFuture = new(2999, 1, 1, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ReturnsTheLogEntriesOfAllAppsNewestFirst()
    {
        LogEntry olderLogEntry = await InsertLogEntryAsync(_appName, _farFuture);
        LogEntry newerLogEntry = await InsertLogEntryAsync(_otherAppName, _farFuture.AddMinutes(1));

        LogEntryPage page = await _database.LogEntryRepository.GetLogEntryPageAsync(1, 2);

        Guid[] expectedIds = [newerLogEntry.Id, olderLogEntry.Id];
        Assert.Equal(expectedIds, page.Entries.Select(logEntry => logEntry.Id));
    }

    [Fact]
    public async Task ReturnsTheLogEntriesOfTheRequestedPage()
    {
        LogEntry oldestLogEntry = await InsertLogEntryAsync(_appName, _farFuture);
        await InsertLogEntryAsync(_otherAppName, _farFuture.AddMinutes(1));
        await InsertLogEntryAsync(_appName, _farFuture.AddMinutes(2));

        LogEntryPage page = await _database.LogEntryRepository.GetLogEntryPageAsync(2, 2);

        Assert.Equal(oldestLogEntry.Id, page.Entries[0].Id);
    }

    [Fact]
    public async Task CountsTheLogEntriesOfAllApps()
    {
        int countBefore = (await _database.LogEntryRepository.GetLogEntryPageAsync(1, 1)).TotalCount;
        await InsertLogEntryAsync(_appName, _farFuture);
        await InsertLogEntryAsync(_otherAppName, _farFuture);

        LogEntryPage page = await _database.LogEntryRepository.GetLogEntryPageAsync(1, 1);

        Assert.Equal(countBefore + 2, page.TotalCount);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _database.LogEntryRepository.DeleteByAppNameAsync(_appName);
        await _database.LogEntryRepository.DeleteByAppNameAsync(_otherAppName);
    }

    private async Task<LogEntry> InsertLogEntryAsync(string appName, DateTimeOffset timestamp)
    {
        LogEntry logEntry = new(appName, "Tests", "Message", LogLevel.Information) { Timestamp = timestamp };
        await _database.LogEntryRepository.InsertAsync(logEntry);
        return logEntry;
    }
}
