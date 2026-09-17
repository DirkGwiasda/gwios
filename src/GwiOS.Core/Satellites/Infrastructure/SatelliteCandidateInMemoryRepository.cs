using System.Collections.Concurrent;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Infrastructure;

/// <summary>
/// Keeps the satellite candidates and their tokens in memory for the lifetime of the application. Registered as
/// singleton.
/// </summary>
internal sealed class SatelliteCandidateInMemoryRepository(ILogger<SatelliteCandidateInMemoryRepository> logger)
    : ISatelliteCandidateRepository
{
    private readonly ConcurrentDictionary<string, StoredCandidate> _candidates = new(StringComparer.Ordinal);
    private readonly ILogger<SatelliteCandidateInMemoryRepository> _logger = logger;

    public void Save(SatelliteCandidate candidate, string token)
        => _candidates[candidate.Name] = new StoredCandidate(candidate, token);

    public SatelliteCandidate? GetByName(string candidateName)
        => _candidates.GetValueOrDefault(candidateName)?.Candidate;

    public string? GetToken(string candidateName)
        => _candidates.GetValueOrDefault(candidateName)?.Token;

    public List<SatelliteCandidate> GetAll()
        => [.. _candidates.Values.Select(storedCandidate => storedCandidate.Candidate)];

    public void Remove(string candidateName)
    {
        if (_candidates.TryRemove(candidateName, out _))
        {
            _logger.LogDebug("Satellite candidate removed from memory.", new Dictionary<string, string> { ["CandidateName"] = candidateName });
        }
    }

    private sealed record StoredCandidate(SatelliteCandidate Candidate, string Token);
}
