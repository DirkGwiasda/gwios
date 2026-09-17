using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;

/// <summary>
/// Checks the values a satellite reported with its ping.
/// </summary>
public interface ISatellitePresenceValidator
{
    /// <summary>
    /// Returns whether the given presence is valid. <paramref name="validationErrors"/> receives one message per
    /// violated rule; it is empty if the presence is valid.
    /// </summary>
    bool IsValid(SatellitePresence presence, out List<string> validationErrors);
}
