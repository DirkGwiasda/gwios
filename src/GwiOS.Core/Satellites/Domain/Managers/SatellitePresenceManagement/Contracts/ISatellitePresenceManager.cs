using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;

/// <summary>
/// Manages what registered satellites report with their pings, and decides whether a satellite is online.
/// </summary>
public interface ISatellitePresenceManager
{
    /// <summary>
    /// Validates the given presence, stamps it with the current time as <see cref="SatellitePresence.LastPingAt"/>
    /// and stores it. Throws a <see cref="SatellitePresenceValidationException"/> if it is invalid.
    /// </summary>
    void RecordPing(SatellitePresence presence);

    /// <summary>
    /// Returns the presence of the satellite with the given name, or <c>null</c> if it has not pinged since GwiOS
    /// started.
    /// </summary>
    SatellitePresence? GetPresence(string satelliteName);

    /// <summary>
    /// Returns whether the satellite of the given presence is online, which it is until 3 minutes after its last
    /// ping.
    /// </summary>
    bool IsOnline(SatellitePresence presence);
}
