using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.RecurringItemManagement.RecurringItemManager;

/// <summary>
/// Covers <c>RecurringItemManager.GetDueDates</c>.
/// </summary>
public sealed class GetDueDatesTests
{
    private readonly RecurringItemManagerProbe _probe = new();

    [Fact]
    public void ReturnsTheDueDatesOfTheSchedulerForTheItemAndMonth()
    {
        RecurringItem item = new()
        {
            Name = "Miete",
            Type = CategoryType.Expense,
            CategoryId = Guid.NewGuid(),
            Amount = 1150m,
            Interval = RecurringInterval.Monthly
        };
        _probe.RecurringItemScheduler.DueDates = [new DateOnly(2026, 10, 1)];

        IReadOnlyList<DateOnly> dueDates = _probe.RecurringItemManager.GetDueDates(item, new DateOnly(2026, 10, 1));

        Assert.Equal([new DateOnly(2026, 10, 1)], dueDates);
        Assert.Equal([(item, new DateOnly(2026, 10, 1))], _probe.RecurringItemScheduler.Requests);
    }
}
