using System.Collections.Concurrent;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Infrastructure;

/// <summary>
/// Keeps the presences of the satellites in memory for the lifetime of the application. Registered as singleton.
/// </summary>
internal sealed class SatellitePresenceInMemoryRepository(ILogger<SatellitePresenceInMemoryRepository> logger)
    : ISatellitePresenceRepository
{
    private readonly ConcurrentDictionary<string, SatellitePresence> _presences = new(StringComparer.Ordinal);
    private readonly ILogger<SatellitePresenceInMemoryRepository> _logger = logger;

    public void Save(SatellitePresence presence)
    {
        // Only the first presence of a satellite is logged; satellites ping every minute.
        if (_presences.TryAdd(presence.SatelliteName, presence))
        {
            _logger.LogDebug("First presence of satellite saved.", new Dictionary<string, string> { ["SatelliteName"] = presence.SatelliteName });
            return;
        }

        _presences[presence.SatelliteName] = presence;
    }

    public SatellitePresence? GetBySatelliteName(string satelliteName)
        => _presences.GetValueOrDefault(satelliteName);
}
