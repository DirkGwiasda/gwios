using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;

/// <summary>
/// Stands in for <see cref="ISatelliteCandidateManager"/> for the workflow tests. It keeps candidates with their tokens
/// in memory and records removals.
/// </summary>
public sealed class SatelliteCandidateManagerFake : ISatelliteCandidateManager
{
    private readonly Dictionary<string, (SatelliteCandidate Candidate, string Token)> _candidates = [];
    private readonly List<string> _removedCandidateNames = [];

    /// <summary>
    /// The kept candidates with their tokens by name.
    /// </summary>
    public IReadOnlyDictionary<string, (SatelliteCandidate Candidate, string Token)> Candidates
        => _candidates;

    /// <summary>
    /// The names of all removed candidates, in the order of removal.
    /// </summary>
    public IReadOnlyList<string> RemovedCandidateNames
        => _removedCandidateNames;

    public void RecordPing(SatelliteCandidate candidate, string token)
        => _candidates[candidate.Name] = (candidate, token);

    public List<SatelliteCandidate> GetAllCandidates()
        => [.. _candidates.Values.Select(entry => entry.Candidate)];

    public string GetTokenForAdoption(string candidateName)
    {
        if (!_candidates.TryGetValue(candidateName, out (SatelliteCandidate Candidate, string Token) entry))
        {
            throw new SatelliteCandidateNotFoundException(candidateName);
        }

        return entry.Candidate.HasTokenChanged
            ? throw new SatelliteCandidateTokenChangedException(candidateName)
            : entry.Token;
    }

    public void RemoveCandidate(string candidateName)
    {
        _candidates.Remove(candidateName);
        _removedCandidateNames.Add(candidateName);
    }
}
