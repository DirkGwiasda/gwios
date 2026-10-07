using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using RecurringItemSchedulerUnderTest = GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.RecurringItemScheduler;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.RecurringItemManagement.RecurringItemScheduler;

/// <summary>
/// Covers <c>RecurringItemScheduler.GetDueDates</c>.
/// </summary>
public sealed class GetDueDatesTests
{
    private readonly RecurringItemSchedulerUnderTest _scheduler = new();

    [Fact]
    public void ReturnsTheDueDayOfAMonthlyItem()
    {
        IReadOnlyList<DateOnly> dueDates = _scheduler.GetDueDates(
            CreateItem(RecurringInterval.Monthly, dueDay: 15),
            new DateOnly(2026, 10, 1));

        Assert.Equal([new DateOnly(2026, 10, 15)], dueDates);
    }

    [Fact]
    public void MovesADueDayBeyondTheEndOfTheMonthToItsLastDay()
    {
        IReadOnlyList<DateOnly> dueDates = _scheduler.GetDueDates(
            CreateItem(RecurringInterval.Monthly, dueDay: 31),
            new DateOnly(2027, 2, 1));

        Assert.Equal([new DateOnly(2027, 2, 28)], dueDates);
    }

    [Fact]
    public void ReturnsFourFixedDaysForAWeeklyItem()
    {
        IReadOnlyList<DateOnly> dueDates = _scheduler.GetDueDates(
            CreateItem(RecurringInterval.Weekly, dueDay: 3),
            new DateOnly(2026, 10, 1));

        Assert.Equal(
            [new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 8), new DateOnly(2026, 10, 15), new DateOnly(2026, 10, 22)],
            dueDates);
    }

    [Theory]
    [InlineData(2, true)]
    [InlineData(3, false)]
    [InlineData(4, false)]
    [InlineData(5, true)]
    [InlineData(11, true)]
    public void ReturnsAQuarterlyItemOnlyEveryThirdMonthFromItsStartMonth(int month, bool isDue)
    {
        // StartMonth 1 is February (zero-based).
        IReadOnlyList<DateOnly> dueDates = _scheduler.GetDueDates(
            CreateItem(RecurringInterval.Quarterly, dueDay: 15, startMonth: 1),
            new DateOnly(2026, month, 1));

        Assert.Equal(isDue, dueDates.Count == 1);
    }

    [Fact]
    public void ReturnsAQuarterlyItemInTheMonthsBeforeItsStartMonth()
    {
        // StartMonth 10 is November; the quarters before fall in August, May and February.
        IReadOnlyList<DateOnly> dueDates = _scheduler.GetDueDates(
            CreateItem(RecurringInterval.Quarterly, dueDay: 1, startMonth: 10),
            new DateOnly(2026, 2, 1));

        Assert.Equal([new DateOnly(2026, 2, 1)], dueDates);
    }

    [Theory]
    [InlineData(4, true)]
    [InlineData(5, false)]
    public void ReturnsAYearlyItemOnlyInItsStartMonth(int month, bool isDue)
    {
        // StartMonth 3 is April (zero-based).
        IReadOnlyList<DateOnly> dueDates = _scheduler.GetDueDates(
            CreateItem(RecurringInterval.Yearly, dueDay: 1, startMonth: 3),
            new DateOnly(2026, month, 1));

        Assert.Equal(isDue, dueDates.Count == 1);
    }

    [Fact]
    public void IgnoresTheDayOfTheGivenDate()
    {
        IReadOnlyList<DateOnly> dueDates = _scheduler.GetDueDates(
            CreateItem(RecurringInterval.Monthly, dueDay: 5),
            new DateOnly(2026, 10, 20));

        Assert.Equal([new DateOnly(2026, 10, 5)], dueDates);
    }

    [Fact]
    public void ReturnsTheDueDatesOfAnInactiveItemAsWell()
    {
        RecurringItem item = CreateItem(RecurringInterval.Monthly, dueDay: 1);
        item.Active = false;

        IReadOnlyList<DateOnly> dueDates = _scheduler.GetDueDates(item, new DateOnly(2026, 10, 1));

        Assert.Equal([new DateOnly(2026, 10, 1)], dueDates);
    }

    private static RecurringItem CreateItem(RecurringInterval interval, int dueDay, int startMonth = 0)
        => new()
        {
            Name = "Posten",
            Type = CategoryType.Expense,
            CategoryId = Guid.NewGuid(),
            Amount = 10m,
            Interval = interval,
            DueDay = dueDay,
            StartMonth = startMonth
        };
}
