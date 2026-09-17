using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview;

internal sealed class SatelliteOverviewWorkflow(
    ISatelliteManager satelliteManager,
    ISatellitePresenceManager satellitePresenceManager,
    ISatelliteCandidateManager satelliteCandidateManager,
    IMeasurementManager measurementManager) : ISatelliteOverviewWorkflow
{
    private readonly ISatelliteManager _satelliteManager = satelliteManager;
    private readonly ISatellitePresenceManager _satellitePresenceManager = satellitePresenceManager;
    private readonly ISatelliteCandidateManager _satelliteCandidateManager = satelliteCandidateManager;
    private readonly IMeasurementManager _measurementManager = measurementManager;

    public async Task<SatelliteOverviewData> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        List<Satellite> satellites = await _satelliteManager.GetAllSatellitesAsync(cancellationToken);
        List<SatelliteOverviewEntry> entries = [.. satellites.Select(CreateEntry)];
        return new SatelliteOverviewData
        {
            Satellites = entries,
            Candidates = _satelliteCandidateManager.GetAllCandidates(),
            LastFetchedAt = FindLastFetchedAt(entries)
        };
    }

    private SatelliteOverviewEntry CreateEntry(Satellite satellite)
    {
        SatellitePresence? presence = _satellitePresenceManager.GetPresence(satellite.Name);
        return new SatelliteOverviewEntry
        {
            Satellite = satellite,
            Presence = presence,
            IsOnline = !satellite.IsLocked && (presence is not null) && _satellitePresenceManager.IsOnline(presence),
            Histories = _measurementManager.GetHistories(satellite.Name)
        };
    }

    // Reads are not logged: the satellite page reads every 5 seconds.
    private static DateTime? FindLastFetchedAt(List<SatelliteOverviewEntry> entries)
        => entries
            .SelectMany(entry => entry.Histories)
            .SelectMany(history => history.RecentSamples)
            .Select(sample => (DateTime?)sample.FetchedAt)
            .Max();
}
