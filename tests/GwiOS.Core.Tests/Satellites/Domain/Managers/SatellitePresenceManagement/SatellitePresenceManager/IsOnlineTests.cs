using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatellitePresenceManagement.SatellitePresenceManager;

/// <summary>
/// Covers <c>SatellitePresenceManager.IsOnline</c>.
/// </summary>
public sealed class IsOnlineTests
{
    private readonly SatellitePresenceManagerProbe _probe = new();

    [Theory]
    [InlineData(0)]
    [InlineData(60)]
    [InlineData(179)]
    public void ReturnsTrue_WithinThreeMinutesAfterThePing(int secondsSincePing)
    {
        SatellitePresence presence = RecordPingSecondsAgo(secondsSincePing);

        Assert.True(_probe.SatellitePresenceManager.IsOnline(presence));
    }

    [Theory]
    [InlineData(180)]
    [InlineData(3600)]
    public void ReturnsFalse_FromThreeMinutesAfterThePing(int secondsSincePing)
    {
        SatellitePresence presence = RecordPingSecondsAgo(secondsSincePing);

        Assert.False(_probe.SatellitePresenceManager.IsOnline(presence));
    }

    private SatellitePresence RecordPingSecondsAgo(int seconds)
    {
        SatellitePresence presence = _probe.Presences.Create();
        _probe.SatellitePresenceManager.RecordPing(presence);
        _probe.TimeProvider.UtcNow += TimeSpan.FromSeconds(seconds);
        return presence;
    }
}
