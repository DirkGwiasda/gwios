using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.Contracts.Models;
using GwiOS.Core.Tests.Satellites.Domain.Managers.MeasurementManagement.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;
using SatelliteOverviewWorkflowUnderTest =
    GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.SatelliteOverviewWorkflow;

namespace GwiOS.Core.Tests.Satellites.Domain.Workflows.SatelliteOverview.SatelliteOverviewWorkflow;

/// <summary>
/// Covers <c>SatelliteOverviewWorkflow.GetOverviewAsync</c>.
/// </summary>
public sealed class GetOverviewAsyncTests
{
    private static readonly DateTime FetchTime = new(2026, 9, 17, 12, 0, 0, DateTimeKind.Utc);

    private readonly SatelliteManagerFake _satelliteManager = new();
    private readonly SatellitePresenceManagerFake _presenceManager = new();
    private readonly SatelliteCandidateManagerFake _candidateManager = new();
    private readonly MeasurementManagerFake _measurementManager = new();
    private readonly TestPresences _presences = new();
    private readonly SatelliteOverviewWorkflowUnderTest _workflow;

    public GetOverviewAsyncTests()
    {
        _workflow = new SatelliteOverviewWorkflowUnderTest(
            _satelliteManager,
            _presenceManager,
            _candidateManager,
            _measurementManager);
    }

    [Fact]
    public async Task CombinesEverySatelliteWithItsPresenceAndHistories()
    {
        _satelliteManager.Add("keller");
        _presenceManager.RecordPing(_presences.Create("keller"));
        List<MeasurementHistory> histories = [CreateHistory("keller", FetchTime)];
        _measurementManager.Histories["keller"] = histories;

        SatelliteOverviewData overview = await _workflow.GetOverviewAsync(TestContext.Current.CancellationToken);

        SatelliteOverviewEntry entry = Assert.Single(overview.Satellites);
        Assert.Equal("keller", entry.Satellite.Name);
        Assert.Same(_presenceManager.Presences["keller"], entry.Presence);
        Assert.True(entry.IsOnline);
        Assert.Same(histories, entry.Histories);
    }

    [Fact]
    public async Task MarksSatellitesOffline_WhenTheyAreLockedOfflineOrNeverPinged()
    {
        _satelliteManager.Add("a-locked", isLocked: true);
        _presenceManager.RecordPing(_presences.Create("a-locked"));
        _satelliteManager.Add("b-offline");
        _presenceManager.RecordPing(_presences.Create("b-offline"));
        _presenceManager.OfflineSatelliteNames.Add("b-offline");
        _satelliteManager.Add("c-never-pinged");

        SatelliteOverviewData overview = await _workflow.GetOverviewAsync(TestContext.Current.CancellationToken);

        Assert.All(overview.Satellites, entry => Assert.False(entry.IsOnline));
        Assert.Null(overview.Satellites[2].Presence);
        Assert.Empty(overview.Satellites[2].Histories);
    }

    [Fact]
    public async Task ContainsTheCandidates()
    {
        SatelliteCandidate candidate = new TestCandidates().Create("garage");
        _candidateManager.RecordPing(candidate, TestCandidates.Token);

        SatelliteOverviewData overview = await _workflow.GetOverviewAsync(TestContext.Current.CancellationToken);

        Assert.Same(candidate, Assert.Single(overview.Candidates));
    }

    [Fact]
    public async Task ReportsTheLatestFetchTimeOfAllSatellites()
    {
        _satelliteManager.Add("garage");
        _satelliteManager.Add("keller");
        _measurementManager.Histories["garage"] = [CreateHistory("garage", FetchTime)];
        _measurementManager.Histories["keller"] = [CreateHistory("keller", FetchTime.AddSeconds(30))];

        SatelliteOverviewData overview = await _workflow.GetOverviewAsync(TestContext.Current.CancellationToken);

        Assert.Equal(FetchTime.AddSeconds(30), overview.LastFetchedAt);
    }

    [Fact]
    public async Task ReportsNoFetchTime_WhenNothingWasFetched()
    {
        _satelliteManager.Add("keller");

        SatelliteOverviewData overview = await _workflow.GetOverviewAsync(TestContext.Current.CancellationToken);

        Assert.Null(overview.LastFetchedAt);
    }

    private static MeasurementHistory CreateHistory(string satelliteName, DateTime fetchedAt)
        => new()
        {
            SatelliteName = satelliteName,
            Sensor = "bme280",
            Quantity = "temperature",
            Unit = "°C",
            RecentSamples = [new MeasurementSample(20, fetchedAt, fetchedAt)],
            LongTermSamples = []
        };
}
