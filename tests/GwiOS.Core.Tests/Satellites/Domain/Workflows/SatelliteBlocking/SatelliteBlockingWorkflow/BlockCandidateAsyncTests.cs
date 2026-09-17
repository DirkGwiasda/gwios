using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using SatelliteBlockingWorkflowUnderTest =
    GwiOS.Core.Satellites.Domain.Workflows.SatelliteBlocking.SatelliteBlockingWorkflow;

namespace GwiOS.Core.Tests.Satellites.Domain.Workflows.SatelliteBlocking.SatelliteBlockingWorkflow;

/// <summary>
/// Covers <c>SatelliteBlockingWorkflow.BlockCandidateAsync</c>.
/// </summary>
public sealed class BlockCandidateAsyncTests
{
    private readonly SatelliteManagerFake _satelliteManager = new();
    private readonly SatelliteCandidateManagerFake _candidateManager = new();
    private readonly LoggerFake<SatelliteBlockingWorkflowUnderTest> _logger = new();
    private readonly SatelliteBlockingWorkflowUnderTest _workflow;

    public BlockCandidateAsyncTests()
    {
        _workflow = new SatelliteBlockingWorkflowUnderTest(_satelliteManager, _candidateManager, _logger);
    }

    [Fact]
    public async Task CreatesALockedSatelliteWithoutTokenAndRemovesTheCandidate()
    {
        _candidateManager.RecordPing(new TestCandidates().Create("fremd"), TestCandidates.Token);

        await _workflow.BlockCandidateAsync("fremd", TestContext.Current.CancellationToken);

        Satellite satellite = Assert.Single(_satelliteManager.Satellites);
        Assert.Equal("fremd", satellite.Name);
        Assert.True(satellite.IsLocked);
        Assert.False(_satelliteManager.Tokens.ContainsKey("fremd"));
        Assert.Equal(["fremd"], _candidateManager.RemovedCandidateNames);
    }

    [Fact]
    public async Task LogsTheBlocking()
    {
        await _workflow.BlockCandidateAsync("fremd", TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_logger.Entries);
        Assert.Equal("Unknown satellite blocked.", logEntry.Message);
    }

    [Fact]
    public async Task ThrowsDuplicateSatelliteExceptionAndKeepsTheCandidate_WhenTheNameIsAlreadyUsed()
    {
        _satelliteManager.Add("fremd");

        await Assert.ThrowsAsync<DuplicateSatelliteException>(
            () => _workflow.BlockCandidateAsync("fremd", TestContext.Current.CancellationToken));

        Assert.Empty(_candidateManager.RemovedCandidateNames);
    }
}
