using GwiOS.Core.Haushaltsbuch.Domain.Workflows.HaushaltsbuchOverview.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Workflows.HaushaltsbuchOverview.Contracts;

/// <summary>
/// Assembles the data behind the Dashboard, Fixkosten, Kategorien and Berichte tabs of the Haushaltsbuch.
/// </summary>
public interface IHaushaltsbuchOverviewWorkflow
{
    /// <summary>
    /// Returns all categories and recurring items, together with the bookings between
    /// <paramref name="fromDate"/> and <paramref name="toDate"/>, both inclusive.
    /// </summary>
    Task<HaushaltsbuchOverviewData> GetOverviewAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);
}
