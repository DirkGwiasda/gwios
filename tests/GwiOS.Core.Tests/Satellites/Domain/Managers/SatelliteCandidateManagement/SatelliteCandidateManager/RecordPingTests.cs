using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.SatelliteCandidateManager;

/// <summary>
/// Covers <c>SatelliteCandidateManager.RecordPing</c>.
/// </summary>
public sealed class RecordPingTests
{
    private readonly SatelliteCandidateManagerProbe _probe = new();

    [Fact]
    public void StoresANewCandidateWithItsTokenAndTheTimeOfThePing()
    {
        SatelliteCandidate candidate = _probe.Candidates.Create();

        _probe.SatelliteCandidateManager.RecordPing(candidate, TestCandidates.Token);

        SatelliteCandidate storedCandidate = _probe.Repository.Candidates["garage"];
        Assert.Equal(_probe.TimeProvider.UtcNow.UtcDateTime, storedCandidate.FirstPingAt);
        Assert.Equal(_probe.TimeProvider.UtcNow.UtcDateTime, storedCandidate.LastPingAt);
        Assert.False(storedCandidate.HasTokenChanged);
        Assert.Equal(TestCandidates.Token, _probe.Repository.Tokens["garage"]);
    }

    [Fact]
    public void KeepsTheFirstPingTimeAndUpdatesTheReportedValues_WhenTheCandidatePingsAgain()
    {
        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create(wifiRssiDbm: -75), TestCandidates.Token);
        DateTime firstPingAt = _probe.TimeProvider.UtcNow.UtcDateTime;
        _probe.AdvanceMinutes(1);

        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create(wifiRssiDbm: -60), TestCandidates.Token);

        SatelliteCandidate storedCandidate = _probe.Repository.Candidates["garage"];
        Assert.Equal(firstPingAt, storedCandidate.FirstPingAt);
        Assert.Equal(_probe.TimeProvider.UtcNow.UtcDateTime, storedCandidate.LastPingAt);
        Assert.Equal(-60, storedCandidate.WifiRssiDbm);
        Assert.False(storedCandidate.HasTokenChanged);
    }

    [Fact]
    public void MarksTheCandidateAndKeepsTheFirstToken_WhenItPingsWithAnotherToken()
    {
        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create(), TestCandidates.Token);

        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create(), TestCandidates.OtherToken);

        Assert.True(_probe.Repository.Candidates["garage"].HasTokenChanged);
        Assert.Equal(TestCandidates.Token, _probe.Repository.Tokens["garage"]);
    }

    [Fact]
    public void KeepsTheMark_WhenTheFirstTokenIsSentAgainLater()
    {
        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create(), TestCandidates.Token);
        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create(), TestCandidates.OtherToken);

        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create(), TestCandidates.Token);

        Assert.True(_probe.Repository.Candidates["garage"].HasTokenChanged);
    }

    [Fact]
    public void LogsAWarningOnce_WhenTheTokenChanges()
    {
        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create(), TestCandidates.Token);
        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create(), TestCandidates.OtherToken);
        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create(), TestCandidates.Token);

        LogEntry warning = Assert.Single(_probe.Logger.Entries, logEntry => logEntry.LogLevel == LogLevel.Warning);
        Assert.Equal("garage", warning.ContextData["CandidateName"]);
    }

    [Fact]
    public void IgnoresTheCandidate_WhenItIsInvalid()
    {
        _probe.Validator.IsCandidateValid = false;

        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create(), "x");

        Assert.Empty(_probe.Repository.Candidates);
        Assert.Empty(_probe.Logger.Entries);
    }

    [Fact]
    public void LogsTheFirstPingOfACandidate()
    {
        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create(), TestCandidates.Token);
        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create(), TestCandidates.Token);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Unknown satellite pinged.", logEntry.Message);
        Assert.DoesNotContain(logEntry.ContextData.Values, value => value == TestCandidates.Token);
    }

    [Fact]
    public void RemovesTheCandidateWithTheOldestPing_WhenTwentyCandidatesAreKept()
    {
        for (int number = 1; number <= 20; number++)
        {
            _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create($"candidate-{number}"), TestCandidates.Token);
            _probe.AdvanceMinutes(0.1);
        }

        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create("newcomer"), TestCandidates.Token);

        Assert.Equal(20, _probe.Repository.Candidates.Count);
        Assert.DoesNotContain("candidate-1", _probe.Repository.Candidates.Keys);
        Assert.Contains("newcomer", _probe.Repository.Candidates.Keys);
    }

    [Fact]
    public void RemovesExpiredCandidates()
    {
        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create("old"), TestCandidates.Token);
        _probe.AdvanceMinutes(10);

        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create("new"), TestCandidates.Token);

        Assert.Equal(["new"], _probe.Repository.Candidates.Keys);
    }

    [Fact]
    public void StartsAnew_WhenAnExpiredCandidatePingsAgainWithAnotherToken()
    {
        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create(), TestCandidates.Token);
        _probe.AdvanceMinutes(11);

        _probe.SatelliteCandidateManager.RecordPing(_probe.Candidates.Create(), TestCandidates.OtherToken);

        Assert.False(_probe.Repository.Candidates["garage"].HasTokenChanged);
        Assert.Equal(TestCandidates.OtherToken, _probe.Repository.Tokens["garage"]);
    }
}
