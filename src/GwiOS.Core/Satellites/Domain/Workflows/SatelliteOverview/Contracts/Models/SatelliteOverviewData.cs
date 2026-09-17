using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.Contracts.Models;

/// <summary>
/// Everything the satellite pages show: the registered satellites with their current state and the unknown satellites
/// that pinged recently.
/// </summary>
public class SatelliteOverviewData
{
    /// <summary>
    /// The registered satellites, ordered by name. Empty if there are none.
    /// </summary>
    public required List<SatelliteOverviewEntry> Satellites { get; init; }

    /// <summary>
    /// The unknown satellites that pinged recently, ordered by their first ping. Empty if there are none.
    /// </summary>
    public required List<SatelliteCandidate> Candidates { get; init; }

    /// <summary>
    /// Point in time measurements were fetched last from any satellite, in UTC, or <c>null</c> if none were fetched
    /// since GwiOS started.
    /// </summary>
    public DateTime? LastFetchedAt { get; init; }
}
