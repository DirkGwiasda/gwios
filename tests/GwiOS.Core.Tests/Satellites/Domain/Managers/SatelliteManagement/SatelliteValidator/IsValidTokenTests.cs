using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using SatelliteValidatorUnderTest = GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.SatelliteValidator;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.SatelliteValidator;

/// <summary>
/// Covers <c>SatelliteValidator.IsValidToken</c>.
/// </summary>
public sealed class IsValidTokenTests
{
    private readonly LoggerFake<SatelliteValidatorUnderTest> _logger = new();
    private readonly SatelliteValidatorUnderTest _validator;

    public IsValidTokenTests()
    {
        _validator = new SatelliteValidatorUnderTest(_logger);
    }

    [Theory]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEF")]
    [InlineData("0123456789-_0123456789-_01234567")]
    public void AcceptsA32CharacterBase64UrlToken(string token)
    {
        Assert.True(_validator.IsValidToken(token));
    }

    [Fact]
    public void AcceptsA128CharacterTokenButNotLonger()
    {
        Assert.True(_validator.IsValidToken(new string('a', 128)));
        Assert.False(_validator.IsValidToken(new string('a', 129)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDE")]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDE+")]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDE=")]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEF\n")]
    public void RejectsATokenThatIsTooShortOrContainsOtherCharacters(string token)
    {
        Assert.False(_validator.IsValidToken(token));
    }

    [Fact]
    public void LogsAWarningWithoutTheToken_WhenTheTokenIsInvalid()
    {
        _validator.IsValidToken("secret");

        LogEntry logEntry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Warning, logEntry.LogLevel);
        Assert.DoesNotContain("secret", logEntry.Message);
        Assert.Empty(logEntry.ContextData);
    }
}
