using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatellitePresenceManagement.SatellitePresenceManager;

/// <summary>
/// Covers <c>SatellitePresenceManager.GetPresence</c>.
/// </summary>
public sealed class GetPresenceTests
{
    private readonly SatellitePresenceManagerProbe _probe = new();

    [Fact]
    public void ReturnsTheLatestPresenceOfTheSatellite()
    {
        SatellitePresence presence = _probe.Presences.Create("keller");
        _probe.SatellitePresenceManager.RecordPing(presence);

        Assert.Same(presence, _probe.SatellitePresenceManager.GetPresence("keller"));
    }

    [Fact]
    public void ReturnsNull_WhenTheSatelliteHasNotPinged()
    {
        Assert.Null(_probe.SatellitePresenceManager.GetPresence("keller"));
    }
}
