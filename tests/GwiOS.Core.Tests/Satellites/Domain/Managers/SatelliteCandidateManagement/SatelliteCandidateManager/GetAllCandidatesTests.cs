using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.SatelliteCandidateManager;

/// <summary>
/// Covers <c>SatelliteCandidateManager.GetAllCandidates</c>.
/// </summary>
public sealed class GetAllCandidatesTests
{
    private readonly SatelliteCandidateManagerProbe _probe = new();

    [Fact]
    public void ReturnsTheCandidatesOrderedByTheirFirstPing()
    {
        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create("zulu"), TestCandidates.Token);
        _probe.AdvanceMinutes(1);
        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create("alpha"), TestCandidates.Token);

        List<SatelliteCandidate> candidates = _probe.SatelliteCandidateManager.GetAllCandidates();

        Assert.Equal(["zulu", "alpha"], candidates.Select(candidate => candidate.Name));
    }

    [Fact]
    public void LeavesOutCandidatesWithoutPingForTenMinutes()
    {
        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create("old"), TestCandidates.Token);
        _probe.AdvanceMinutes(5);
        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create("new"), TestCandidates.Token);
        _probe.AdvanceMinutes(5);

        List<SatelliteCandidate> candidates = _probe.SatelliteCandidateManager.GetAllCandidates();

        Assert.Equal("new", Assert.Single(candidates).Name);
    }

    [Fact]
    public void ReturnsAnEmptyList_WhenThereAreNoCandidates()
    {
        Assert.Empty(_probe.SatelliteCandidateManager.GetAllCandidates());
    }
}
