using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Workflows.MeasurementPolling.Contracts;

namespace GwiOS.Core.Satellites.Domain.Workflows.MeasurementPolling;

internal sealed class MeasurementPollingWorkflow(
    ISatelliteManager satelliteManager,
    ISatellitePresenceManager satellitePresenceManager,
    IMeasurementManager measurementManager,
    ILogger<MeasurementPollingWorkflow> logger) : IMeasurementPollingWorkflow
{
    private readonly ISatelliteManager _satelliteManager = satelliteManager;
    private readonly ISatellitePresenceManager _satellitePresenceManager = satellitePresenceManager;
    private readonly IMeasurementManager _measurementManager = measurementManager;
    private readonly ILogger<MeasurementPollingWorkflow> _logger = logger;

    public async Task PollAllAsync(CancellationToken cancellationToken = default)
    {
        List<Satellite> satellites = await _satelliteManager.GetAllSatellitesAsync(cancellationToken);
        foreach (Satellite satellite in satellites.Where(satellite => !satellite.IsLocked))
        {
            if (GetOnlinePresence(satellite.Name) is { } presence)
            {
                await PollSatelliteAsync(presence, cancellationToken);
            }
        }
    }

    private SatellitePresence? GetOnlinePresence(string satelliteName)
        => _satellitePresenceManager.GetPresence(satelliteName) is { } presence
            && _satellitePresenceManager.IsOnline(presence)
                ? presence
                : null;

    private async Task PollSatelliteAsync(SatellitePresence presence, CancellationToken cancellationToken)
    {
        try
        {
            await FetchAndRecordAsync(presence, cancellationToken);
        }
        catch (Exception exception) when (exception is SatelliteNotFoundException
            or SatelliteTokenRejectedException
            or MeasurementFetchException
            or MeasurementValidationException)
        {
            // The managers already logged the cause; this only records that the satellite was skipped.
            _logger.LogDebug(
                "Satellite skipped while fetching measurements.",
                new Dictionary<string, string>
                {
                    ["SatelliteName"] = presence.SatelliteName,
                    ["Reason"] = exception.GetType().Name
                });
        }
    }

    private async Task FetchAndRecordAsync(SatellitePresence presence, CancellationToken cancellationToken)
    {
        string token = await _satelliteManager.GetTokenAsync(presence.SatelliteName, cancellationToken);
        MeasurementReading reading = await _measurementManager.FetchReadingAsync(
            presence.SatelliteName,
            presence.Endpoint,
            token,
            cancellationToken);
        _measurementManager.RecordReading(presence.SatelliteName, reading);
    }
}
