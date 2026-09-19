using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.RecurringItemManagement.RecurringItemManager;

/// <summary>
/// Covers <c>RecurringItemManager.GetAllRecurringItemsAsync</c>.
/// </summary>
public sealed class GetAllRecurringItemsAsyncTests
{
    private readonly RecurringItemManagerProbe _probe = new();

    [Fact]
    public async Task ReturnsTheRecurringItemsOfTheRepositoryInItsOrder()
    {
        RecurringItem expenseItem = new()
        {
            Name = "Miete", Type = CategoryType.Expense, CategoryId = Guid.CreateVersion7(), Amount = 1150m,
            Interval = RecurringInterval.Monthly
        };
        RecurringItem incomeItem = new()
        {
            Name = "Gehalt", Type = CategoryType.Income, CategoryId = Guid.CreateVersion7(), Amount = 3200m,
            Interval = RecurringInterval.Monthly
        };
        await _probe.RecurringItemRepository.InsertAsync(expenseItem, TestContext.Current.CancellationToken);
        await _probe.RecurringItemRepository.InsertAsync(incomeItem, TestContext.Current.CancellationToken);

        List<RecurringItem> recurringItems =
            await _probe.RecurringItemManager.GetAllRecurringItemsAsync(TestContext.Current.CancellationToken);

        Assert.Equal([incomeItem, expenseItem], recurringItems);
    }

    [Fact]
    public async Task ReturnsAnEmptyList_WhenNoRecurringItemsAreStored()
    {
        List<RecurringItem> recurringItems =
            await _probe.RecurringItemManager.GetAllRecurringItemsAsync(TestContext.Current.CancellationToken);

        Assert.Empty(recurringItems);
    }
}
