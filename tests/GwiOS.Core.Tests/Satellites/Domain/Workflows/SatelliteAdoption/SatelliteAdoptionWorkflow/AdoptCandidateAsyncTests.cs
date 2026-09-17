using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using SatelliteAdoptionWorkflowUnderTest =
    GwiOS.Core.Satellites.Domain.Workflows.SatelliteAdoption.SatelliteAdoptionWorkflow;

namespace GwiOS.Core.Tests.Satellites.Domain.Workflows.SatelliteAdoption.SatelliteAdoptionWorkflow;

/// <summary>
/// Covers <c>SatelliteAdoptionWorkflow.AdoptCandidateAsync</c>.
/// </summary>
public sealed class AdoptCandidateAsyncTests
{
    private readonly SatelliteCandidateManagerFake _candidateManager = new();
    private readonly SatelliteManagerFake _satelliteManager = new();
    private readonly LoggerFake<SatelliteAdoptionWorkflowUnderTest> _logger = new();
    private readonly TestCandidates _candidates = new();
    private readonly SatelliteAdoptionWorkflowUnderTest _workflow;

    public AdoptCandidateAsyncTests()
    {
        _workflow = new SatelliteAdoptionWorkflowUnderTest(_candidateManager, _satelliteManager, _logger);
    }

    [Fact]
    public async Task CreatesTheSatelliteWithTheTokenOfTheCandidateAndRemovesTheCandidate()
    {
        _candidateManager.RecordPing(_candidates.Create("garage"), TestCandidates.Token);

        await _workflow.AdoptCandidateAsync("garage", "Garage", TestContext.Current.CancellationToken);

        Satellite satellite = Assert.Single(_satelliteManager.Satellites);
        Assert.Equal("garage", satellite.Name);
        Assert.Equal("Garage", satellite.DisplayName);
        Assert.Equal(TestCandidates.Token, _satelliteManager.Tokens["garage"]);
        Assert.Equal(["garage"], _candidateManager.RemovedCandidateNames);
    }

    [Fact]
    public async Task LogsTheAdoption()
    {
        _candidateManager.RecordPing(_candidates.Create("garage"), TestCandidates.Token);

        await _workflow.AdoptCandidateAsync("garage", null, TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_logger.Entries);
        Assert.Equal("Unknown satellite adopted.", logEntry.Message);
        Assert.Equal("garage", logEntry.ContextData["SatelliteName"]);
    }

    [Fact]
    public async Task ThrowsSatelliteCandidateNotFoundException_WhenNoSuchCandidateExists()
    {
        await Assert.ThrowsAsync<SatelliteCandidateNotFoundException>(
            () => _workflow.AdoptCandidateAsync("garage", null, TestContext.Current.CancellationToken));

        Assert.Empty(_satelliteManager.Satellites);
    }

    [Fact]
    public async Task ThrowsSatelliteCandidateTokenChangedExceptionAndKeepsTheCandidate_WhenTheTokenChanged()
    {
        SatelliteCandidate candidate = _candidates.Create("garage");
        candidate.HasTokenChanged = true;
        _candidateManager.RecordPing(candidate, TestCandidates.Token);

        await Assert.ThrowsAsync<SatelliteCandidateTokenChangedException>(
            () => _workflow.AdoptCandidateAsync("garage", null, TestContext.Current.CancellationToken));

        Assert.Empty(_satelliteManager.Satellites);
        Assert.Empty(_candidateManager.RemovedCandidateNames);
    }

    [Fact]
    public async Task ThrowsDuplicateSatelliteExceptionAndKeepsTheCandidate_WhenTheNameIsAlreadyUsed()
    {
        _satelliteManager.Add("garage");
        _candidateManager.RecordPing(_candidates.Create("garage"), TestCandidates.Token);

        await Assert.ThrowsAsync<DuplicateSatelliteException>(
            () => _workflow.AdoptCandidateAsync("garage", null, TestContext.Current.CancellationToken));

        Assert.Empty(_candidateManager.RemovedCandidateNames);
    }
}
