using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.CrossCutting.Logging.Infrastructure.LogEntryPostgresRepository;

/// <summary>
/// Covers <c>LogEntryPostgresRepository.InsertAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class InsertAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly PostgresTestDatabase _database = database;
    private readonly string _appName = $"test-{Guid.NewGuid():N}";

    [Fact]
    public async Task StoresAllPropertiesOfTheLogEntry()
    {
        LogEntry logEntry = new(
            _appName,
            "Tests.DataSource",
            "Message",
            LogLevel.Warning,
            new Dictionary<string, string> { ["Key"] = "Value" })
        {
            Timestamp = new DateTimeOffset(2026, 9, 11, 7, 30, 15, 123, TimeSpan.Zero)
        };

        await _database.LogEntryRepository.InsertAsync(logEntry);

        List<LogEntry> storedLogEntries = await _database.LogEntryRepository.GetAllLogEntriesByAppAsync(_appName);
        LogEntry storedLogEntry = Assert.Single(storedLogEntries);
        Assert.Equal(logEntry.Id, storedLogEntry.Id);
        Assert.Equal(logEntry.Timestamp, storedLogEntry.Timestamp);
        Assert.Equal(logEntry.AppName, storedLogEntry.AppName);
        Assert.Equal(logEntry.DataSource, storedLogEntry.DataSource);
        Assert.Equal(logEntry.Message, storedLogEntry.Message);
        Assert.Equal(logEntry.LogLevel, storedLogEntry.LogLevel);
        Assert.Equal(logEntry.ContextData, storedLogEntry.ContextData);
    }

    [Fact]
    public async Task StoresTheSameInstant_WhenTheTimestampHasAnOffset()
    {
        DateTimeOffset timestamp = new(2026, 9, 11, 9, 30, 15, TimeSpan.FromHours(2));
        LogEntry logEntry = new(_appName, "Tests", "Message", LogLevel.Information) { Timestamp = timestamp };

        await _database.LogEntryRepository.InsertAsync(logEntry);

        List<LogEntry> storedLogEntries = await _database.LogEntryRepository.GetAllLogEntriesByAppAsync(_appName);
        LogEntry storedLogEntry = Assert.Single(storedLogEntries);
        Assert.Equal(timestamp.UtcDateTime, storedLogEntry.Timestamp.UtcDateTime);
    }

    [Fact]
    public async Task StoresEmptyContextData_WhenTheLogEntryHasNoContextData()
    {
        LogEntry logEntry = new(_appName, "Tests", "Message", LogLevel.Information);

        await _database.LogEntryRepository.InsertAsync(logEntry);

        List<LogEntry> storedLogEntries = await _database.LogEntryRepository.GetAllLogEntriesByAppAsync(_appName);
        LogEntry storedLogEntry = Assert.Single(storedLogEntries);
        Assert.Empty(storedLogEntry.ContextData);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
        => await _database.LogEntryRepository.DeleteByAppNameAsync(_appName);
}
