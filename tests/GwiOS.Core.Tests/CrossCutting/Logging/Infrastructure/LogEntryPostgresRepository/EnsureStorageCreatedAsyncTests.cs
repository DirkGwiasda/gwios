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

        List<LogEntry> storedLogEntries = await _database.LogEntryRepository.GetAllLogEntriesByAppAsync(_appName);
        LogEntry storedLogEntry = Assert.Single(storedLogEntries);
        Assert.Equal(logEntry.Id, storedLogEntry.Id);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
        => await _database.LogEntryRepository.DeleteByAppNameAsync(_appName);
}
