using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement;

internal sealed class RecurringItemScheduler : IRecurringItemScheduler
{
    // Weekly items have no weekday of their own; like in the design template they fall due on four fixed days.
    private static readonly int[] WeeklyDueDays = [1, 8, 15, 22];

    // Mirrors the interval rules of the design template: weekly and monthly items fall due every month, quarterly
    // and yearly items are phased from their start month. A due day beyond the end of the month moves to its last day.
    public IReadOnlyList<DateOnly> GetDueDates(RecurringItem recurringItem, DateOnly month)
    {
        if (!IsDueInMonth(recurringItem, month.Month - 1))
        {
            return [];
        }

        int lastDay = DateTime.DaysInMonth(month.Year, month.Month);
        IEnumerable<int> dueDays = recurringItem.Interval == RecurringInterval.Weekly
            ? WeeklyDueDays
            : [recurringItem.DueDay];
        return [.. dueDays.Select(day => new DateOnly(month.Year, month.Month, Math.Min(day, lastDay)))];
    }

    private static bool IsDueInMonth(RecurringItem recurringItem, int monthIndex0)
        => recurringItem.Interval switch
        {
            RecurringInterval.Weekly or RecurringInterval.Monthly => true,
            RecurringInterval.Quarterly => (((monthIndex0 - recurringItem.StartMonth) % 3) + 3) % 3 == 0,
            RecurringInterval.Yearly => monthIndex0 == recurringItem.StartMonth,
            _ => false
        };
}
