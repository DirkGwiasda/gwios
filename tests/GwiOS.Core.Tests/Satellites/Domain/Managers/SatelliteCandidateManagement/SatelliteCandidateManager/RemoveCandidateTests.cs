using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.SatelliteCandidateManager;

/// <summary>
/// Covers <c>SatelliteCandidateManager.RemoveCandidate</c>.
/// </summary>
public sealed class RemoveCandidateTests
{
    private readonly SatelliteCandidateManagerProbe _probe = new();

    [Fact]
    public void RemovesTheCandidateAndItsToken()
    {
        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create(), TestCandidates.Token);

        _probe.SatelliteCandidateManager.RemoveCandidate("garage");

        Assert.Empty(_probe.Repository.Candidates);
        Assert.Empty(_probe.Repository.Tokens);
    }

    [Fact]
    public void DoesNothing_WhenNoSuchCandidateExists()
    {
        _probe.SatelliteCandidateManager.RemoveCandidate("garage");

        Assert.Empty(_probe.Repository.Candidates);
    }

    [Fact]
    public void LogsTheRemoval()
    {
        _probe.SatelliteCandidateManager.RemoveCandidate("garage");

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal("Unknown satellite removed.", logEntry.Message);
        Assert.Equal("garage", logEntry.ContextData["CandidateName"]);
    }
}
