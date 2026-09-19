using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using RecurringItemUnderTest = GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models.RecurringItem;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models.RecurringItem;

/// <summary>
/// Covers the construction of <c>RecurringItem</c> and the defaults it assigns.
/// </summary>
public sealed class ConstructorTests
{
    [Fact]
    public void AssignsAVersion7Id()
    {
        RecurringItemUnderTest recurringItem = CreateRecurringItem();

        Assert.Equal(7, recurringItem.Id.Version);
    }

    [Fact]
    public void AssignsADifferentIdToEachRecurringItem()
    {
        RecurringItemUnderTest first = CreateRecurringItem();
        RecurringItemUnderTest second = CreateRecurringItem();

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void DefaultsToDueDayOneAndStartMonthZeroAndActive()
    {
        RecurringItemUnderTest recurringItem = CreateRecurringItem();

        Assert.Equal(1, recurringItem.DueDay);
        Assert.Equal(0, recurringItem.StartMonth);
        Assert.True(recurringItem.Active);
    }

    private static RecurringItemUnderTest CreateRecurringItem()
        => new()
        {
            Name = "Miete",
            Type = CategoryType.Expense,
            CategoryId = Guid.CreateVersion7(),
            Amount = 1150m,
            Interval = RecurringInterval.Monthly
        };
}
