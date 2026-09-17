using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.Contracts.Models;

namespace GwiOS.WebUI.Tests.TestInfrastructure;

/// <summary>
/// Stands in for <see cref="ISatelliteOverviewWorkflow"/>: returns the satellites of a <see cref="SatelliteManagerFake"/>
/// together with the presences, online states, histories and candidates the test sets, and counts the calls.
/// </summary>
public sealed class SatelliteOverviewWorkflowFake(SatelliteManagerFake satelliteManager) : ISatelliteOverviewWorkflow
{
    private readonly SatelliteManagerFake _satelliteManager = satelliteManager;

    /// <summary>
    /// The entries by satellite name that replace the plain entry of a satellite. A satellite without entry is offline,
    /// has no presence and no histories.
    /// </summary>
    public Dictionary<string, SatelliteOverviewEntry> Entries { get; } = [];

    /// <summary>
    /// The candidates of the overview.
    /// </summary>
    public List<SatelliteCandidate> Candidates { get; } = [];

    /// <summary>
    /// The fetch time of the overview.
    /// </summary>
    public DateTime? LastFetchedAt { get; set; }

    /// <summary>
    /// How often the overview was read.
    /// </summary>
    public int CallCount { get; private set; }

    public async Task<SatelliteOverviewData> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        CallCount++;
        return new SatelliteOverviewData
        {
            Satellites =
            [
                .. (await _satelliteManager.GetAllSatellitesAsync(cancellationToken)).Select(satellite =>
                    Entries.GetValueOrDefault(satellite.Name) ?? new SatelliteOverviewEntry { Satellite = satellite, Histories = [] })
            ],
            Candidates = [.. Candidates],
            LastFetchedAt = LastFetchedAt
        };
    }
}
