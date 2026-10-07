namespace GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts.Models;

/// <summary>
/// The forecast of the recurring items over consecutive months.
/// </summary>
public sealed class RecurringItemForecastData
{
    /// <summary>
    /// One entry per forecast month, in chronological order; a month without due items is included as well.
    /// </summary>
    public required IReadOnlyList<RecurringItemForecastMonth> Months { get; init; }
}
