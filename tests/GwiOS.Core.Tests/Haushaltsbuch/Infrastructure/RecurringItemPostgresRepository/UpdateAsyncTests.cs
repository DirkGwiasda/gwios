using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.RecurringItemPostgresRepository;

/// <summary>
/// Covers <c>RecurringItemPostgresRepository.UpdateAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class UpdateAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly PostgresTestDatabase _database = database;
    private readonly IRecurringItemRepository _recurringItemRepository = database.RecurringItemRepository;
    private readonly TestRecurringItems _testRecurringItems = new(database.CategoryRepository, database.RecurringItemRepository);

    [Fact]
    public async Task OverwritesAllChangeablePropertiesOfTheStoredRecurringItem()
    {
        RecurringItem recurringItem = await _testRecurringItems.CreateAsync(TestContext.Current.CancellationToken);
        await _recurringItemRepository.InsertAsync(recurringItem, TestContext.Current.CancellationToken);
        RecurringItem changed = new()
        {
            Id = recurringItem.Id, Name = _testRecurringItems.CreateUniqueName(), Type = recurringItem.Type,
            CategoryId = recurringItem.CategoryId, Amount = 999.99m, Interval = RecurringInterval.Yearly,
            DueDay = 20, StartMonth = 3, Active = false
        };

        await _recurringItemRepository.UpdateAsync(changed, TestContext.Current.CancellationToken);

        RecurringItem? stored = await _testRecurringItems.FindStoredAsync(recurringItem.Id);
        Assert.NotNull(stored);
        Assert.Equal(changed.Name, stored.Name);
        Assert.Equal(changed.Amount, stored.Amount);
        Assert.Equal(changed.Interval, stored.Interval);
        Assert.Equal(changed.DueDay, stored.DueDay);
        Assert.Equal(changed.StartMonth, stored.StartMonth);
        Assert.False(stored.Active);
    }

    [Fact]
    public async Task ThrowsRecurringItemNotFoundException_WhenTheRecurringItemIsNotStored()
    {
        RecurringItem recurringItem = await _testRecurringItems.CreateAsync(TestContext.Current.CancellationToken);

        RecurringItemNotFoundException exception = await Assert.ThrowsAsync<RecurringItemNotFoundException>(
            () => _recurringItemRepository.UpdateAsync(recurringItem, TestContext.Current.CancellationToken));

        Assert.Equal(recurringItem.Id, exception.RecurringItemId);
    }

    [Fact]
    public async Task LogsAWarningWithTheRecurringItemId_WhenTheRecurringItemIsNotStored()
    {
        RecurringItem recurringItem = await _testRecurringItems.CreateAsync(TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<RecurringItemNotFoundException>(
            () => _recurringItemRepository.UpdateAsync(recurringItem, TestContext.Current.CancellationToken));

        Assert.Contains(
            _database.RecurringItemRepositoryLogger.Entries,
            logEntry => (logEntry.LogLevel == LogLevel.Warning)
                && (logEntry.ContextData.GetValueOrDefault("RecurringItemId") == recurringItem.Id.ToString()));
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testRecurringItems.DisposeAsync();
}
