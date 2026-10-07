using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;

/// <summary>
/// Stands in for <see cref="IRecurringItemScheduler"/>: returns <see cref="DueDates"/> for every item and month and
/// records what it was asked for.
/// </summary>
public sealed class RecurringItemSchedulerFake : IRecurringItemScheduler
{
    /// <summary>
    /// The due dates every call returns.
    /// </summary>
    public IReadOnlyList<DateOnly> DueDates { get; set; } = [];

    /// <summary>
    /// The recurring items and months the scheduler was asked for, in call order.
    /// </summary>
    public List<(RecurringItem RecurringItem, DateOnly Month)> Requests { get; } = [];

    public IReadOnlyList<DateOnly> GetDueDates(RecurringItem recurringItem, DateOnly month)
    {
        Requests.Add((recurringItem, month));
        return DueDates;
    }
}
