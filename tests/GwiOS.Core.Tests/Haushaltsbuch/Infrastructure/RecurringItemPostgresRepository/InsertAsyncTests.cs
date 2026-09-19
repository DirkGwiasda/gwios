using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.Haushaltsbuch.Infrastructure.RecurringItemPostgresRepository;

/// <summary>
/// Covers <c>RecurringItemPostgresRepository.InsertAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class InsertAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly IRecurringItemRepository _recurringItemRepository = database.RecurringItemRepository;
    private readonly TestRecurringItems _testRecurringItems = new(database.CategoryRepository, database.RecurringItemRepository);

    [Fact]
    public async Task StoresAllPropertiesOfTheRecurringItem()
    {
        RecurringItem recurringItem = await _testRecurringItems.CreateAsync(TestContext.Current.CancellationToken);
        recurringItem.Amount = 44.9m;
        recurringItem.Interval = RecurringInterval.Quarterly;
        recurringItem.DueDay = 15;
        recurringItem.StartMonth = 1;
        recurringItem.Active = false;

        await _recurringItemRepository.InsertAsync(recurringItem, TestContext.Current.CancellationToken);

        RecurringItem? stored = await _testRecurringItems.FindStoredAsync(recurringItem.Id);
        Assert.NotNull(stored);
        Assert.Equal(recurringItem.Name, stored.Name);
        Assert.Equal(recurringItem.Type, stored.Type);
        Assert.Equal(recurringItem.CategoryId, stored.CategoryId);
        Assert.Equal(recurringItem.Amount, stored.Amount);
        Assert.Equal(recurringItem.Interval, stored.Interval);
        Assert.Equal(recurringItem.DueDay, stored.DueDay);
        Assert.Equal(recurringItem.StartMonth, stored.StartMonth);
        Assert.False(stored.Active);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testRecurringItems.DisposeAsync();
}
