using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.MeasurementManagement.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;
using MeasurementPollingWorkflowUnderTest =
    GwiOS.Core.Satellites.Domain.Workflows.MeasurementPolling.MeasurementPollingWorkflow;

namespace GwiOS.Core.Tests.Satellites.Domain.Workflows.MeasurementPolling.MeasurementPollingWorkflow;

/// <summary>
/// Covers <c>MeasurementPollingWorkflow.PollAllAsync</c>.
/// </summary>
public sealed class PollAllAsyncTests
{
    private readonly SatelliteManagerFake _satelliteManager = new();
    private readonly SatellitePresenceManagerFake _presenceManager = new();
    private readonly MeasurementManagerFake _measurementManager = new();
    private readonly LoggerFake<MeasurementPollingWorkflowUnderTest> _logger = new();
    private readonly TestPresences _presences = new();
    private readonly MeasurementPollingWorkflowUnderTest _workflow;

    public PollAllAsyncTests()
    {
        _workflow = new MeasurementPollingWorkflowUnderTest(_satelliteManager, _presenceManager, _measurementManager, _logger);
    }

    [Fact]
    public async Task FetchesFromTheReportedEndpointWithTheTokenAndRecordsTheReading()
    {
        AddOnlineSatellite("keller", "keller-token");

        await _workflow.PollAllAsync(TestContext.Current.CancellationToken);

        Assert.Equal(("keller", "http://192.168.178.33", "keller-token"), Assert.Single(_measurementManager.Fetches));
        (string satelliteName, _) = Assert.Single(_measurementManager.RecordedReadings);
        Assert.Equal("keller", satelliteName);
    }

    [Fact]
    public async Task SkipsLockedSatellitesSatellitesWithoutPresenceAndOfflineSatellites()
    {
        AddOnlineSatellite("online", "token");
        _satelliteManager.Add("locked", isLocked: true);
        _presenceManager.RecordPing(_presences.Create("locked"));
        _satelliteManager.Add("never-pinged");
        _satelliteManager.Add("offline");
        _presenceManager.RecordPing(_presences.Create("offline"));
        _presenceManager.OfflineSatelliteNames.Add("offline");

        await _workflow.PollAllAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["online"], _measurementManager.Fetches.Select(fetch => fetch.SatelliteName));
    }

    [Fact]
    public async Task ContinuesWithTheOtherSatellites_WhenOneCannotBeFetched()
    {
        AddOnlineSatellite("a-unreachable", "token");
        AddOnlineSatellite("b-reachable", "token");
        _measurementManager.UnreachableSatelliteNames.Add("a-unreachable");

        await _workflow.PollAllAsync(TestContext.Current.CancellationToken);

        Assert.Equal("b-reachable", Assert.Single(_measurementManager.RecordedReadings).SatelliteName);
        LogEntry logEntry = Assert.Single(_logger.Entries);
        Assert.Equal("a-unreachable", logEntry.ContextData["SatelliteName"]);
        Assert.Equal("MeasurementFetchException", logEntry.ContextData["Reason"]);
    }

    [Fact]
    public async Task SkipsASatelliteWhoseTokenIsRejected()
    {
        _satelliteManager.Add("no-token", token: null);
        _presenceManager.RecordPing(_presences.Create("no-token"));

        await _workflow.PollAllAsync(TestContext.Current.CancellationToken);

        Assert.Empty(_measurementManager.Fetches);
        Assert.Equal("SatelliteTokenRejectedException", Assert.Single(_logger.Entries).ContextData["Reason"]);
    }

    private void AddOnlineSatellite(string name, string token)
    {
        _satelliteManager.Add(name, token);
        SatellitePresence presence = _presences.Create(name);
        _presenceManager.RecordPing(presence);
    }
}
