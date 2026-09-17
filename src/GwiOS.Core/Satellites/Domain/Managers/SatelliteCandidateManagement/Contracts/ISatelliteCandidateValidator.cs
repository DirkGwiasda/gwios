using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;

/// <summary>
/// Checks the values an unknown satellite pinged with.
/// </summary>
public interface ISatelliteCandidateValidator
{
    /// <summary>
    /// Returns whether the given candidate and the token it pinged with are valid: name and token in the format of
    /// registered satellites, and plausible reported values.
    /// </summary>
    bool IsValid(SatelliteCandidate candidate, string token);
}
