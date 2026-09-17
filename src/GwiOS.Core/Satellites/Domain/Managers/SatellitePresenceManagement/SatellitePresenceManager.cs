using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement;

internal sealed class SatellitePresenceManager(
    ISatellitePresenceRepository satellitePresenceRepository,
    ISatellitePresenceValidator satellitePresenceValidator,
    TimeProvider timeProvider,
    ILogger<SatellitePresenceManager> logger) : ISatellitePresenceManager
{
    // Satellites ping every 60 seconds, so a satellite is offline after three missed pings.
    private static readonly TimeSpan OfflineAfter = TimeSpan.FromMinutes(3);

    private readonly ISatellitePresenceRepository _satellitePresenceRepository = satellitePresenceRepository;
    private readonly ISatellitePresenceValidator _satellitePresenceValidator = satellitePresenceValidator;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<SatellitePresenceManager> _logger = logger;

    public void RecordPing(SatellitePresence presence)
    {
        ThrowIfInvalid(presence);
        bool wasOnline = IsKnownAndOnline(presence.SatelliteName);
        presence.LastPingAt = _timeProvider.GetUtcNow().UtcDateTime;
        _satellitePresenceRepository.Save(presence);
        if (!wasOnline)
        {
            LogCameOnline(presence);
        }
    }

    public SatellitePresence? GetPresence(string satelliteName)
        => _satellitePresenceRepository.GetBySatelliteName(satelliteName);

    public bool IsOnline(SatellitePresence presence)
        => (_timeProvider.GetUtcNow().UtcDateTime - presence.LastPingAt) < OfflineAfter;

    private void ThrowIfInvalid(SatellitePresence presence)
    {
        if (!_satellitePresenceValidator.IsValid(presence, out List<string> validationErrors))
        {
            throw new SatellitePresenceValidationException(presence.SatelliteName, validationErrors);
        }
    }

    private bool IsKnownAndOnline(string satelliteName)
        => _satellitePresenceRepository.GetBySatelliteName(satelliteName) is { } storedPresence
            && IsOnline(storedPresence);

    // Only coming online is logged; a log entry for every ping would flood the log.
    private void LogCameOnline(SatellitePresence presence)
        => _logger.LogInformation(
            "Satellite is online.",
            new Dictionary<string, string>
            {
                ["SatelliteName"] = presence.SatelliteName,
                ["Endpoint"] = presence.Endpoint,
                ["Firmware"] = presence.Firmware
            });
}
