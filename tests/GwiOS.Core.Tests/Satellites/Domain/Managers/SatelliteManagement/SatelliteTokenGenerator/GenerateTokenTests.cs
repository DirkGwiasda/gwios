using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using SatelliteTokenGeneratorUnderTest = GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.SatelliteTokenGenerator;
using SatelliteValidatorUnderTest = GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.SatelliteValidator;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.SatelliteTokenGenerator;

/// <summary>
/// Covers <c>SatelliteTokenGenerator.GenerateToken</c>.
/// </summary>
public sealed class GenerateTokenTests
{
    private readonly SatelliteTokenGeneratorUnderTest _generator = new(new LoggerFake<SatelliteTokenGeneratorUnderTest>());

    [Fact]
    public void ReturnsA43CharacterToken()
    {
        Assert.Equal(43, _generator.GenerateToken().Length);
    }

    [Fact]
    public void ReturnsATokenInTheFormatSatellitesAccept()
    {
        SatelliteValidatorUnderTest validator = new(new LoggerFake<SatelliteValidatorUnderTest>());

        Assert.True(validator.IsValidToken(_generator.GenerateToken()));
    }

    [Fact]
    public void ReturnsADifferentTokenEveryTime()
    {
        HashSet<string> tokens = [.. Enumerable.Range(0, 100).Select(_ => _generator.GenerateToken())];

        Assert.Equal(100, tokens.Count);
    }
}
