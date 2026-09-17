using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;
using SatelliteCandidateValidatorUnderTest =
    GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.SatelliteCandidateValidator;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.SatelliteCandidateValidator;

/// <summary>
/// Covers <c>SatelliteCandidateValidator.IsValid</c>.
/// </summary>
public sealed class IsValidTests
{
    private readonly LoggerFake<SatelliteCandidateValidatorUnderTest> _logger = new();
    private readonly TestCandidates _candidates = new();
    private readonly SatelliteCandidateValidatorUnderTest _validator;

    public IsValidTests()
    {
        _validator = new SatelliteCandidateValidatorUnderTest(_logger);
    }

    [Fact]
    public void AcceptsAPlausibleCandidateWithAValidToken()
    {
        Assert.True(_validator.IsValid(_candidates.Create(), TestCandidates.Token));
        Assert.Empty(_logger.Entries);
    }

    [Theory]
    [InlineData("Garage")]
    [InlineData("")]
    [InlineData("garage\n")]
    [InlineData("abcdefghijklmnopqrstuvwxyz0123456")]
    public void RejectsAnInvalidName(string name)
    {
        Assert.False(_validator.IsValid(_candidates.Create(name), TestCandidates.Token));
    }

    [Theory]
    [InlineData("")]
    [InlineData("too-short")]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEF0123456789+")]
    public void RejectsAnInvalidToken(string token)
    {
        Assert.False(_validator.IsValid(_candidates.Create(), token));
    }

    [Theory]
    [InlineData("https://192.168.178.40", "ESP32", "0.1.0", -70)]
    [InlineData("http://192.168.178.40/path", "ESP32", "0.1.0", -70)]
    [InlineData("http://192.168.178.40", "", "0.1.0", -70)]
    [InlineData("http://192.168.178.40", "ESP32", "", -70)]
    [InlineData("http://192.168.178.40", "ESP32", "0.1.0", 5)]
    public void RejectsImplausibleReportedValues(string endpoint, string board, string firmware, int wifiRssiDbm)
    {
        SatelliteCandidate candidate = new()
        {
            Name = "garage",
            Endpoint = endpoint,
            Board = board,
            Firmware = firmware,
            WifiRssiDbm = wifiRssiDbm
        };

        Assert.False(_validator.IsValid(candidate, TestCandidates.Token));
    }

    [Fact]
    public void LogsAWarningWithoutNameAndToken_WhenTheCandidateIsInvalid()
    {
        _validator.IsValid(_candidates.Create("Garage"), TestCandidates.Token);

        LogEntry logEntry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Warning, logEntry.LogLevel);
        Assert.Empty(logEntry.ContextData);
    }
}
