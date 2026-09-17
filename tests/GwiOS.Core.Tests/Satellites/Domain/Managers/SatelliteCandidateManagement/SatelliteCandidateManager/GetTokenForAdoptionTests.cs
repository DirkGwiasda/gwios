using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Exceptions;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.SatelliteCandidateManager;

/// <summary>
/// Covers <c>SatelliteCandidateManager.GetTokenForAdoption</c>.
/// </summary>
public sealed class GetTokenForAdoptionTests
{
    private readonly SatelliteCandidateManagerProbe _probe = new();

    [Fact]
    public void ReturnsTheTokenTheCandidatePingedWith()
    {
        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create(), TestCandidates.Token);

        Assert.Equal(TestCandidates.Token, _probe.SatelliteCandidateManager.GetTokenForAdoption("garage"));
    }

    [Fact]
    public void ThrowsSatelliteCandidateNotFoundException_WhenNoSuchCandidateExists()
    {
        SatelliteCandidateNotFoundException exception = Assert.Throws<SatelliteCandidateNotFoundException>(
            () => _probe.SatelliteCandidateManager.GetTokenForAdoption("garage"));

        Assert.Equal("garage", exception.CandidateName);
    }

    [Fact]
    public void ThrowsSatelliteCandidateNotFoundException_WhenTheCandidateHasExpired()
    {
        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create(), TestCandidates.Token);
        _probe.AdvanceMinutes(10);

        Assert.Throws<SatelliteCandidateNotFoundException>(
            () => _probe.SatelliteCandidateManager.GetTokenForAdoption("garage"));
    }

    [Fact]
    public void ThrowsSatelliteCandidateTokenChangedException_WhenTheCandidatePingedWithDifferentTokens()
    {
        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create(), TestCandidates.Token);
        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create(), TestCandidates.OtherToken);

        Assert.Throws<SatelliteCandidateTokenChangedException>(
            () => _probe.SatelliteCandidateManager.GetTokenForAdoption("garage"));
    }
}
