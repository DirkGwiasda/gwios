using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Workflows.SatellitePing.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;
using SatellitePingWorkflowUnderTest = GwiOS.Core.Satellites.Domain.Workflows.SatellitePing.SatellitePingWorkflow;

namespace GwiOS.Core.Tests.Satellites.Domain.Workflows.SatellitePing.SatellitePingWorkflow;

/// <summary>
/// Covers <c>SatellitePingWorkflow.ProcessPingAsync</c>.
/// </summary>
public sealed class ProcessPingAsyncTests
{
    private readonly SatelliteManagerFake _satelliteManager = new();
    private readonly SatellitePresenceManagerFake _presenceManager = new();
    private readonly SatelliteCandidateManagerFake _candidateManager = new();
    private readonly SatellitePingWorkflowUnderTest _workflow;

    public ProcessPingAsyncTests()
    {
        _workflow = new SatellitePingWorkflowUnderTest(
            _satelliteManager,
            _presenceManager,
            _candidateManager,
            new LoggerFake<SatellitePingWorkflowUnderTest>());
    }

    [Fact]
    public async Task RecordsWhatTheSatelliteReported_WhenTheTokenIsAccepted()
    {
        _satelliteManager.Add("arbeitszimmer", token: "right-token");

        await _workflow.ProcessPingAsync(CreatePing("arbeitszimmer", "right-token"), TestContext.Current.CancellationToken);

        SatellitePresence presence = _presenceManager.Presences["arbeitszimmer"];
        Assert.Equal("http://192.168.178.33", presence.Endpoint);
        Assert.Equal("ESP32 DevKit C", presence.Board);
        Assert.Equal("0.1.0", presence.Firmware);
        Assert.Equal(-70, presence.WifiRssiDbm);
        SatelliteSensor sensor = Assert.Single(presence.Sensors);
        Assert.Equal("bme280", sensor.Type);
        Assert.Equal(["temperature", "humidity", "pressure"], sensor.Quantities);
        Assert.Empty(_candidateManager.Candidates);
    }

    [Fact]
    public async Task RecordsTheSatelliteAsCandidateAndRethrows_WhenTheNameIsUnknown()
    {
        await Assert.ThrowsAsync<SatelliteNotFoundException>(
            () => _workflow.ProcessPingAsync(CreatePing("garage", "some-token"), TestContext.Current.CancellationToken));

        (SatelliteCandidate candidate, string token) = _candidateManager.Candidates["garage"];
        Assert.Equal("some-token", token);
        Assert.Equal("http://192.168.178.33", candidate.Endpoint);
        Assert.Equal("ESP32 DevKit C", candidate.Board);
        Assert.Equal("0.1.0", candidate.Firmware);
        Assert.Equal(-70, candidate.WifiRssiDbm);
        Assert.Empty(_presenceManager.Presences);
    }

    [Fact]
    public async Task RethrowsAndRecordsNothing_WhenTheTokenIsRejected()
    {
        _satelliteManager.Add("arbeitszimmer", token: "right-token");

        await Assert.ThrowsAsync<SatelliteTokenRejectedException>(
            () => _workflow.ProcessPingAsync(CreatePing("arbeitszimmer", "wrong-token"), TestContext.Current.CancellationToken));

        Assert.Empty(_presenceManager.Presences);
        Assert.Empty(_candidateManager.Candidates);
    }

    [Fact]
    public async Task Rethrows_WhenTheReportedValuesAreInvalid()
    {
        _satelliteManager.Add("arbeitszimmer", token: "right-token");
        _presenceManager.RejectsPresences = true;

        await Assert.ThrowsAsync<SatellitePresenceValidationException>(
            () => _workflow.ProcessPingAsync(CreatePing("arbeitszimmer", "right-token"), TestContext.Current.CancellationToken));
    }

    private static SatellitePingData CreatePing(string satelliteName, string token)
        => new()
        {
            SatelliteName = satelliteName,
            Token = token,
            Endpoint = "http://192.168.178.33",
            Board = "ESP32 DevKit C",
            Firmware = "0.1.0",
            WifiRssiDbm = -70,
            Sensors = [new SatellitePingSensor { Type = "bme280", Quantities = ["temperature", "humidity", "pressure"] }]
        };
}
