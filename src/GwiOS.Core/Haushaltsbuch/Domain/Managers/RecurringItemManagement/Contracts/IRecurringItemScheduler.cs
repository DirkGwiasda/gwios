using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;

/// <summary>
/// Works out on which days of a month a recurring item falls due.
/// </summary>
public interface IRecurringItemScheduler
{
    /// <summary>
    /// Returns the days in the month of <paramref name="month"/> on which the given recurring item falls due, in
    /// ascending order; empty if it does not fall due in that month. Whether the item is active is not considered.
    /// </summary>
    IReadOnlyList<DateOnly> GetDueDates(RecurringItem recurringItem, DateOnly month);
}
