using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Workflows.SatellitePing.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.SatellitePing.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Workflows.SatellitePing;

internal sealed class SatellitePingWorkflow(
    ISatelliteManager satelliteManager,
    ISatellitePresenceManager satellitePresenceManager,
    ISatelliteCandidateManager satelliteCandidateManager,
    ILogger<SatellitePingWorkflow> logger) : ISatellitePingWorkflow
{
    private readonly ISatelliteManager _satelliteManager = satelliteManager;
    private readonly ISatellitePresenceManager _satellitePresenceManager = satellitePresenceManager;
    private readonly ISatelliteCandidateManager _satelliteCandidateManager = satelliteCandidateManager;
    private readonly ILogger<SatellitePingWorkflow> _logger = logger;

    public async Task ProcessPingAsync(SatellitePingData ping, CancellationToken cancellationToken = default)
    {
        try
        {
            await _satelliteManager.VerifyTokenAsync(ping.SatelliteName, ping.Token, cancellationToken);
        }
        catch (SatelliteNotFoundException)
        {
            _logger.LogDebug("Ping of unknown satellite is recorded as candidate.");
            _satelliteCandidateManager.RecordPing(ToCandidate(ping), ping.Token);
            throw;
        }

        _satellitePresenceManager.RecordPing(ToPresence(ping));
    }

    private static SatelliteCandidate ToCandidate(SatellitePingData ping)
        => new()
        {
            Name = ping.SatelliteName,
            Endpoint = ping.Endpoint,
            Board = ping.Board,
            Firmware = ping.Firmware,
            WifiRssiDbm = ping.WifiRssiDbm
        };

    private static SatellitePresence ToPresence(SatellitePingData ping)
        => new()
        {
            SatelliteName = ping.SatelliteName,
            Endpoint = ping.Endpoint,
            Board = ping.Board,
            Firmware = ping.Firmware,
            WifiRssiDbm = ping.WifiRssiDbm,
            Sensors = [.. ping.Sensors.Select(ToSensor)]
        };

    private static SatelliteSensor ToSensor(SatellitePingSensor sensor)
        => new() { Type = sensor.Type, Quantities = [.. sensor.Quantities] };
}
