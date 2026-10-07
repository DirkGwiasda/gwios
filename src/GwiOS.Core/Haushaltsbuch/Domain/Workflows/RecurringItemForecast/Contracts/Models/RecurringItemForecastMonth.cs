namespace GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts.Models;

/// <summary>
/// The recurring items falling due in one month of a forecast, with their totals.
/// </summary>
public sealed class RecurringItemForecastMonth
{
    /// <summary>
    /// The first day of the month.
    /// </summary>
    public required DateOnly Month { get; init; }

    /// <summary>
    /// The due income items, ordered by date and then by name.
    /// </summary>
    public required IReadOnlyList<RecurringItemForecastEntry> Incomes { get; init; }

    /// <summary>
    /// The due expense items, ordered by date and then by name.
    /// </summary>
    public required IReadOnlyList<RecurringItemForecastEntry> Expenses { get; init; }

    /// <summary>
    /// The sum of <see cref="Incomes"/>.
    /// </summary>
    public required decimal IncomeTotal { get; init; }

    /// <summary>
    /// The sum of <see cref="Expenses"/>.
    /// </summary>
    public required decimal ExpenseTotal { get; init; }

    /// <summary>
    /// <see cref="IncomeTotal"/> minus <see cref="ExpenseTotal"/>.
    /// </summary>
    public required decimal Balance { get; init; }
}
