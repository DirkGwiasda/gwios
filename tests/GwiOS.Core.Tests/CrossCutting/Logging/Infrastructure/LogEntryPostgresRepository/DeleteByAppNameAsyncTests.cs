using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.CrossCutting.Logging.Infrastructure.LogEntryPostgresRepository;

/// <summary>
/// Covers <c>LogEntryPostgresRepository.DeleteByAppNameAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class DeleteByAppNameAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly PostgresTestDatabase _database = database;
    private readonly string _appName = $"test-{Guid.NewGuid():N}";
    private readonly string _otherAppName = $"test-{Guid.NewGuid():N}";

    [Fact]
    public async Task DeletesAllLogEntriesOfTheGivenApp()
    {
        await InsertLogEntryAsync(_appName);
        await InsertLogEntryAsync(_appName);

        await _database.LogEntryRepository.DeleteByAppNameAsync(_appName);

        Assert.Empty(await _database.LogEntryRepository.GetAllLogEntriesByAppAsync(_appName));
    }

    [Fact]
    public async Task KeepsTheLogEntriesOfOtherApps()
    {
        await InsertLogEntryAsync(_appName);
        await InsertLogEntryAsync(_otherAppName);

        await _database.LogEntryRepository.DeleteByAppNameAsync(_appName);

        Assert.Single(await _database.LogEntryRepository.GetAllLogEntriesByAppAsync(_otherAppName));
    }

    [Fact]
    public async Task Succeeds_WhenTheAppHasNoLogEntries()
    {
        Exception? exception = await Record.ExceptionAsync(
            () => _database.LogEntryRepository.DeleteByAppNameAsync(_appName));

        Assert.Null(exception);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _database.LogEntryRepository.DeleteByAppNameAsync(_appName);
        await _database.LogEntryRepository.DeleteByAppNameAsync(_otherAppName);
    }

    private async Task InsertLogEntryAsync(string appName)
        => await _database.LogEntryRepository.InsertAsync(
            new LogEntry(appName, "Tests", "Message", LogLevel.Information));
}
