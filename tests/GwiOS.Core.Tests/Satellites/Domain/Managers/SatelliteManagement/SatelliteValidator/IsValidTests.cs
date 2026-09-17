using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using SatelliteValidatorUnderTest = GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.SatelliteValidator;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.SatelliteValidator;

/// <summary>
/// Covers <c>SatelliteValidator.IsValid</c>.
/// </summary>
public sealed class IsValidTests
{
    private readonly LoggerFake<SatelliteValidatorUnderTest> _logger = new();
    private readonly SatelliteValidatorUnderTest _validator;

    public IsValidTests()
    {
        _validator = new SatelliteValidatorUnderTest(_logger);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("arbeitszimmer")]
    [InlineData("kinderzimmer-mila")]
    [InlineData("sat-01")]
    [InlineData("abcdefghijklmnopqrstuvwxyz012345")]
    public void AcceptsANameOutOfLowercaseLettersDigitsAndHyphens(string name)
    {
        bool isValid = _validator.IsValid(new Satellite { Name = name }, out List<string> validationErrors);

        Assert.True(isValid);
        Assert.Empty(validationErrors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Keller")]
    [InlineData("keller zimmer")]
    [InlineData("küche")]
    [InlineData("keller_1")]
    [InlineData("keller\n")]
    [InlineData("abcdefghijklmnopqrstuvwxyz0123456")]
    public void RejectsAnInvalidName(string name)
    {
        bool isValid = _validator.IsValid(new Satellite { Name = name }, out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Contains("name", Assert.Single(validationErrors));
    }

    [Theory]
    [InlineData("Kinderzimmer Mila")]
    [InlineData("K")]
    public void AcceptsADisplayName(string displayName)
    {
        Assert.True(_validator.IsValid(new Satellite { Name = "keller", DisplayName = displayName }, out _));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("Keller\tunten")]
    public void RejectsAnEmptyDisplayNameOrOneWithControlCharacters(string displayName)
    {
        bool isValid = _validator.IsValid(
            new Satellite { Name = "keller", DisplayName = displayName },
            out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Contains("display name", Assert.Single(validationErrors));
    }

    [Fact]
    public void RejectsADisplayNameLongerThan64Characters()
    {
        Assert.True(_validator.IsValid(new Satellite { Name = "keller", DisplayName = new string('x', 64) }, out _));
        Assert.False(_validator.IsValid(new Satellite { Name = "keller", DisplayName = new string('x', 65) }, out _));
    }

    [Fact]
    public void LogsAWarningWithTheViolatedRules_WhenTheSatelliteIsInvalid()
    {
        _validator.IsValid(new Satellite { Name = "Keller" }, out _);

        LogEntry logEntry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Warning, logEntry.LogLevel);
        Assert.Contains("name", logEntry.ContextData["ValidationErrors"]);
    }
}
