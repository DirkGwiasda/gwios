using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts.Models;

namespace GwiOS.WebUI.Tests.TestInfrastructure;

/// <summary>
/// Stands in for <see cref="IRecurringItemForecastWorkflow"/>: returns <see cref="Forecast"/> and records the
/// requested first month and month count.
/// </summary>
public sealed class RecurringItemForecastWorkflowFake : IRecurringItemForecastWorkflow
{
    /// <summary>
    /// The forecast every call returns.
    /// </summary>
    public RecurringItemForecastData Forecast { get; set; } = new() { Months = [] };

    /// <summary>
    /// The first months and month counts requested, in call order.
    /// </summary>
    public List<(DateOnly FirstMonth, int MonthCount)> Requests { get; } = [];

    public Task<RecurringItemForecastData> GetForecastAsync(
        DateOnly firstMonth,
        int monthCount,
        CancellationToken cancellationToken = default)
    {
        Requests.Add((firstMonth, monthCount));
        return Task.FromResult(Forecast);
    }
}
