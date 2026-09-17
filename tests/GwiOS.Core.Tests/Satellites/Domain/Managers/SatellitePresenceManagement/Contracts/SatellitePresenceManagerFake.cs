using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;

/// <summary>
/// Stands in for <see cref="ISatellitePresenceManager"/> for the workflow tests. It stores every recorded presence and
/// considers every satellite online that is not listed in <see cref="OfflineSatelliteNames"/>.
/// </summary>
public sealed class SatellitePresenceManagerFake : ISatellitePresenceManager
{
    private readonly Dictionary<string, SatellitePresence> _presences = [];

    /// <summary>
    /// The recorded presences by satellite name.
    /// </summary>
    public IReadOnlyDictionary<string, SatellitePresence> Presences
        => _presences;

    /// <summary>
    /// The names of the satellites that are offline.
    /// </summary>
    public HashSet<string> OfflineSatelliteNames { get; } = [];

    /// <summary>
    /// Whether every presence is rejected as invalid. <c>false</c> by default.
    /// </summary>
    public bool RejectsPresences { get; set; }

    public void RecordPing(SatellitePresence presence)
    {
        if (RejectsPresences)
        {
            throw new SatellitePresenceValidationException(presence.SatelliteName, ["Invalid."]);
        }

        _presences[presence.SatelliteName] = presence;
    }

    public SatellitePresence? GetPresence(string satelliteName)
        => _presences.GetValueOrDefault(satelliteName);

    public bool IsOnline(SatellitePresence presence)
        => !OfflineSatelliteNames.Contains(presence.SatelliteName);
}
