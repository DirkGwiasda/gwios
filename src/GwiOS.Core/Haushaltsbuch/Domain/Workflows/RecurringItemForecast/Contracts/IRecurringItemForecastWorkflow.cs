using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts;

/// <summary>
/// Forecasts when the active recurring items of the Haushaltsbuch fall due in the coming months, independently of
/// whether they have been booked already.
/// </summary>
public interface IRecurringItemForecastWorkflow
{
    /// <summary>
    /// Returns, for the month of <paramref name="firstMonth"/> and the following months up to
    /// <paramref name="monthCount"/> months in total, every due date of every active recurring item, split into
    /// income and expenses. Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="monthCount"/> is
    /// less than 1.
    /// </summary>
    Task<RecurringItemForecastData> GetForecastAsync(
        DateOnly firstMonth,
        int monthCount,
        CancellationToken cancellationToken = default);
}
