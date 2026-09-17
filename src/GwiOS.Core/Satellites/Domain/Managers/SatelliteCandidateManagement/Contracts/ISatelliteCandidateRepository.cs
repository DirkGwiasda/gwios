using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;

/// <summary>
/// Keeps satellite candidates together with the token each one pinged with first. Safe to use from several threads at
/// once.
/// </summary>
public interface ISatelliteCandidateRepository
{
    /// <summary>
    /// Stores the given candidate with the given token, replacing a candidate with the same name.
    /// </summary>
    void Save(SatelliteCandidate candidate, string token);

    /// <summary>
    /// Returns the candidate with the given name, or <c>null</c> if none is stored.
    /// </summary>
    SatelliteCandidate? GetByName(string candidateName);

    /// <summary>
    /// Returns the token stored with the candidate of the given name, or <c>null</c> if no such candidate is stored.
    /// </summary>
    string? GetToken(string candidateName);

    /// <summary>
    /// Returns all stored candidates in no particular order. Empty if none is stored.
    /// </summary>
    List<SatelliteCandidate> GetAll();

    /// <summary>
    /// Removes the candidate with the given name and its token. Does nothing if no such candidate is stored.
    /// </summary>
    void Remove(string candidateName);
}
