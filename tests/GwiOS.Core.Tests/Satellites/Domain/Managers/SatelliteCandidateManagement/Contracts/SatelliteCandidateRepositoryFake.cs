using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;

/// <summary>
/// Stands in for <see cref="ISatelliteCandidateRepository"/> by keeping candidates and tokens in dictionaries.
/// </summary>
public sealed class SatelliteCandidateRepositoryFake : ISatelliteCandidateRepository
{
    private readonly Dictionary<string, SatelliteCandidate> _candidates = [];
    private readonly Dictionary<string, string> _tokens = [];

    /// <summary>
    /// The stored candidates by name.
    /// </summary>
    public IReadOnlyDictionary<string, SatelliteCandidate> Candidates
        => _candidates;

    /// <summary>
    /// The stored tokens by candidate name.
    /// </summary>
    public IReadOnlyDictionary<string, string> Tokens
        => _tokens;

    public void Save(SatelliteCandidate candidate, string token)
    {
        _candidates[candidate.Name] = candidate;
        _tokens[candidate.Name] = token;
    }

    public SatelliteCandidate? GetByName(string candidateName)
        => _candidates.GetValueOrDefault(candidateName);

    public string? GetToken(string candidateName)
        => _tokens.GetValueOrDefault(candidateName);

    public List<SatelliteCandidate> GetAll()
        => [.. _candidates.Values];

    public void Remove(string candidateName)
    {
        _candidates.Remove(candidateName);
        _tokens.Remove(candidateName);
    }
}
