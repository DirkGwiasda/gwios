using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.CrossCutting.Logging.Infrastructure.LogEntryPostgresRepository;

/// <summary>
/// Covers <c>LogEntryPostgresRepository.EnsureStorageCreatedAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class EnsureStorageCreatedAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly PostgresTestDatabase _database = database;
    private readonly string _appName = $"test-{Guid.NewGuid():N}";

    [Fact]
    public async Task KeepsExistingLogEntries_WhenTheTablesAlreadyExist()
    {
        LogEntry logEntry = new(_appName, "Tests", "Message", LogLevel.Information);
        await _database.LogEntryRepository.InsertAsync(logEntry);

        await _database.LogEntryRepository.EnsureStorageCreatedAsync();

        IReadOnlyList<LogEntry> storedLogEntries = await ReadStoredLogEntriesAsync();
        LogEntry storedLogEntry = Assert.Single(storedLogEntries);
        Assert.Equal(logEntry.Id, storedLogEntry.Id);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
        => await _database.LogEntryRepository.DeleteByAppNameAsync(_appName);

    private async Task<IReadOnlyList<LogEntry>> ReadStoredLogEntriesAsync()
        => (await _database.LogEntryRepository.GetLogEntryPageByAppAsync(_appName, 1, 10)).Entries;
}
