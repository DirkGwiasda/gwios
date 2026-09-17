using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;

/// <summary>
/// Manages the satellites that ping GwiOS with an unknown name. At most 20 candidates are kept; a candidate expires 10
/// minutes after its latest ping.
/// </summary>
public interface ISatelliteCandidateManager
{
    /// <summary>
    /// Records a ping of the given candidate with the given token and stamps it with the current time. A ping with
    /// invalid values is ignored. A later ping with another token marks the candidate with
    /// <see cref="SatelliteCandidate.HasTokenChanged"/> and keeps the first token. If 20 candidates are kept already,
    /// the one with the oldest ping makes room.
    /// </summary>
    void RecordPing(SatelliteCandidate candidate, string token);

    /// <summary>
    /// Returns all candidates that have not expired, ordered by their first ping. Empty if there are none.
    /// </summary>
    List<SatelliteCandidate> GetAllCandidates();

    /// <summary>
    /// Returns the token of the candidate with the given name, so the candidate can be registered with it. Throws a
    /// <see cref="SatelliteCandidateNotFoundException"/> if no such candidate exists or it has expired, and a
    /// <see cref="SatelliteCandidateTokenChangedException"/> if it pinged with different tokens.
    /// </summary>
    string GetTokenForAdoption(string candidateName);

    /// <summary>
    /// Removes the candidate with the given name. Does nothing if no such candidate exists.
    /// </summary>
    void RemoveCandidate(string candidateName);
}
