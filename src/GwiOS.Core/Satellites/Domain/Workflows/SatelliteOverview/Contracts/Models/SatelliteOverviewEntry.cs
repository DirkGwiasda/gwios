using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.Contracts.Models;

/// <summary>
/// A registered satellite together with what it reported last and the values fetched from it.
/// </summary>
public class SatelliteOverviewEntry
{
    /// <summary>
    /// The registered satellite.
    /// </summary>
    public required Satellite Satellite { get; init; }

    /// <summary>
    /// What the satellite reported with its latest ping, or <c>null</c> if it has not pinged since GwiOS started.
    /// </summary>
    public SatellitePresence? Presence { get; init; }

    /// <summary>
    /// Whether the satellite is online: not locked and pinged within the last 3 minutes.
    /// </summary>
    public bool IsOnline { get; init; }

    /// <summary>
    /// The histories of the values fetched from the satellite, ordered by sensor and quantity. Empty if none were
    /// fetched since GwiOS started.
    /// </summary>
    public required List<MeasurementHistory> Histories { get; init; }
}
