using GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.Contracts;

/// <summary>
/// Puts together what the satellite pages show.
/// </summary>
public interface ISatelliteOverviewWorkflow
{
    /// <summary>
    /// Returns all registered satellites with their presence, online state and histories, and the unknown satellites
    /// that pinged recently.
    /// </summary>
    Task<SatelliteOverviewData> GetOverviewAsync(CancellationToken cancellationToken = default);
}
