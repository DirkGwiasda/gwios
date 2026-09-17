using System.Security.Cryptography;
using System.Text;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement;

internal sealed class SatelliteCandidateManager(
    ISatelliteCandidateRepository satelliteCandidateRepository,
    ISatelliteCandidateValidator satelliteCandidateValidator,
    TimeProvider timeProvider,
    ILogger<SatelliteCandidateManager> logger) : ISatelliteCandidateManager
{
    // Anyone who reaches the ping endpoint can invent names, so the number of candidates is limited.
    private const int MaxCandidateCount = 20;

    private static readonly TimeSpan ExpiresAfter = TimeSpan.FromMinutes(10);

    private readonly ISatelliteCandidateRepository _satelliteCandidateRepository = satelliteCandidateRepository;
    private readonly ISatelliteCandidateValidator _satelliteCandidateValidator = satelliteCandidateValidator;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<SatelliteCandidateManager> _logger = logger;

    public void RecordPing(SatelliteCandidate candidate, string token)
    {
        if (!_satelliteCandidateValidator.IsValid(candidate, token))
        {
            return;
        }

        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
        RemoveExpiredCandidates(now);
        if (_satelliteCandidateRepository.GetByName(candidate.Name) is { } storedCandidate)
        {
            UpdateCandidate(storedCandidate, candidate, token, now);
            return;
        }

        AddCandidate(candidate, token, now);
    }

    public List<SatelliteCandidate> GetAllCandidates()
    {
        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;
        return
        [
            .. _satelliteCandidateRepository.GetAll()
                .Where(candidate => !IsExpired(candidate, now))
                .OrderBy(candidate => candidate.FirstPingAt)
        ];
    }

    public string GetTokenForAdoption(string candidateName)
    {
        SatelliteCandidate? candidate = _satelliteCandidateRepository.GetByName(candidateName);
        string? token = _satelliteCandidateRepository.GetToken(candidateName);
        if ((candidate is null) || (token is null) || IsExpired(candidate, _timeProvider.GetUtcNow().UtcDateTime))
        {
            throw new SatelliteCandidateNotFoundException(candidateName);
        }

        if (candidate.HasTokenChanged)
        {
            throw new SatelliteCandidateTokenChangedException(candidateName);
        }

        return token;
    }

    public void RemoveCandidate(string candidateName)
    {
        _satelliteCandidateRepository.Remove(candidateName);
        _logger.LogInformation("Unknown satellite removed.", CreateCandidateNameData(candidateName));
    }

    private void UpdateCandidate(SatelliteCandidate storedCandidate, SatelliteCandidate candidate, string token, DateTime now)
    {
        // The first token is kept: a second sender with the same name must not replace the token of the first.
        string storedToken = _satelliteCandidateRepository.GetToken(storedCandidate.Name) ?? token;
        bool hasTokenChanged = !AreEqualInConstantTime(storedToken, token);
        if (hasTokenChanged && !storedCandidate.HasTokenChanged)
        {
            _logger.LogWarning("Unknown satellite pinged with another token.", CreateCandidateNameData(candidate.Name));
        }

        candidate.FirstPingAt = storedCandidate.FirstPingAt;
        candidate.LastPingAt = now;
        candidate.HasTokenChanged = storedCandidate.HasTokenChanged || hasTokenChanged;
        _satelliteCandidateRepository.Save(candidate, storedToken);
    }

    private void AddCandidate(SatelliteCandidate candidate, string token, DateTime now)
    {
        MakeRoomForCandidate();
        candidate.FirstPingAt = now;
        candidate.LastPingAt = now;
        candidate.HasTokenChanged = false;
        _satelliteCandidateRepository.Save(candidate, token);
        _logger.LogInformation("Unknown satellite pinged.", CreateCandidateNameData(candidate.Name));
    }

    private void MakeRoomForCandidate()
    {
        List<SatelliteCandidate> candidates = _satelliteCandidateRepository.GetAll();
        foreach (SatelliteCandidate candidate in candidates
            .OrderBy(candidate => candidate.LastPingAt)
            .Take(candidates.Count - MaxCandidateCount + 1))
        {
            _satelliteCandidateRepository.Remove(candidate.Name);
        }
    }

    private void RemoveExpiredCandidates(DateTime now)
    {
        foreach (SatelliteCandidate candidate in _satelliteCandidateRepository.GetAll()
            .Where(candidate => IsExpired(candidate, now)))
        {
            _satelliteCandidateRepository.Remove(candidate.Name);
        }
    }

    private static bool IsExpired(SatelliteCandidate candidate, DateTime now)
        => (now - candidate.LastPingAt) >= ExpiresAfter;

    private static bool AreEqualInConstantTime(string expected, string actual)
        => CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(actual));

    private static Dictionary<string, string> CreateCandidateNameData(string candidateName)
        => new() { ["CandidateName"] = candidateName };
}
