using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;

/// <summary>
/// Stands in for <see cref="ISatellitePresenceRepository"/> by keeping presences in a dictionary.
/// </summary>
public sealed class SatellitePresenceRepositoryFake : ISatellitePresenceRepository
{
    private readonly Dictionary<string, SatellitePresence> _presences = [];

    /// <summary>
    /// The stored presences by satellite name.
    /// </summary>
    public IReadOnlyDictionary<string, SatellitePresence> Presences
        => _presences;

    public void Save(SatellitePresence presence)
        => _presences[presence.SatelliteName] = presence;

    public SatellitePresence? GetBySatelliteName(string satelliteName)
        => _presences.GetValueOrDefault(satelliteName);
}
