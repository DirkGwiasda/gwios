using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;

/// <summary>
/// Keeps the latest presence of every satellite. Safe to use from several threads at once.
/// </summary>
public interface ISatellitePresenceRepository
{
    /// <summary>
    /// Stores the given presence, replacing the presence stored for the same satellite.
    /// </summary>
    void Save(SatellitePresence presence);

    /// <summary>
    /// Returns the presence of the satellite with the given name, or <c>null</c> if none is stored.
    /// </summary>
    SatellitePresence? GetBySatelliteName(string satelliteName);
}
