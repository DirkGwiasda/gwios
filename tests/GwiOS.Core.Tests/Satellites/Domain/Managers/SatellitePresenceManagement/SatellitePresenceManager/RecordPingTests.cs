using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatellitePresenceManagement.SatellitePresenceManager;

/// <summary>
/// Covers <c>SatellitePresenceManager.RecordPing</c>.
/// </summary>
public sealed class RecordPingTests
{
    private readonly SatellitePresenceManagerProbe _probe = new();

    [Fact]
    public void StoresThePresenceStampedWithTheCurrentTime()
    {
        SatellitePresence presence = _probe.Presences.Create();

        _probe.SatellitePresenceManager.RecordPing(presence);

        Assert.Same(presence, _probe.Repository.Presences["arbeitszimmer"]);
        Assert.Equal(_probe.TimeProvider.UtcNow.UtcDateTime, presence.LastPingAt);
    }

    [Fact]
    public void ReplacesThePreviousPresenceOfTheSatellite()
    {
        _probe.SatellitePresenceManager.RecordPing(_probe.Presences.Create());
        SatellitePresence laterPresence = _probe.Presences.Create();

        _probe.SatellitePresenceManager.RecordPing(laterPresence);

        Assert.Same(laterPresence, Assert.Single(_probe.Repository.Presences).Value);
    }

    [Fact]
    public void ThrowsSatellitePresenceValidationExceptionAndStoresNothing_WhenThePresenceIsInvalid()
    {
        _probe.Validator.ValidationErrors = ["Invalid endpoint."];

        SatellitePresenceValidationException exception = Assert.Throws<SatellitePresenceValidationException>(
            () => _probe.SatellitePresenceManager.RecordPing(_probe.Presences.Create()));

        Assert.Equal("arbeitszimmer", exception.SatelliteName);
        Assert.Equal(["Invalid endpoint."], exception.ValidationErrors);
        Assert.Empty(_probe.Repository.Presences);
    }

    [Fact]
    public void LogsThatTheSatelliteIsOnline_WhenItPingsForTheFirstTime()
    {
        _probe.SatellitePresenceManager.RecordPing(_probe.Presences.Create());

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Satellite is online.", logEntry.Message);
        Assert.Equal("arbeitszimmer", logEntry.ContextData["SatelliteName"]);
        Assert.Equal("http://192.168.178.33", logEntry.ContextData["Endpoint"]);
    }

    [Fact]
    public void LogsNothing_WhenAnOnlineSatellitePingsAgain()
    {
        _probe.SatellitePresenceManager.RecordPing(_probe.Presences.Create());
        _probe.TimeProvider.UtcNow += TimeSpan.FromMinutes(1);

        _probe.SatellitePresenceManager.RecordPing(_probe.Presences.Create());

        Assert.Single(_probe.Logger.Entries);
    }

    [Fact]
    public void LogsThatTheSatelliteIsOnlineAgain_WhenItPingsAfterBeingOffline()
    {
        _probe.SatellitePresenceManager.RecordPing(_probe.Presences.Create());
        _probe.TimeProvider.UtcNow += TimeSpan.FromMinutes(5);

        _probe.SatellitePresenceManager.RecordPing(_probe.Presences.Create());

        Assert.Equal(2, _probe.Logger.Entries.Count);
    }
}
