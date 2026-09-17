using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using Microsoft.AspNetCore.DataProtection;
using SatelliteTokenProtectorUnderTest = GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.SatelliteTokenProtector;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.SatelliteTokenProtector;

/// <summary>
/// Covers <c>SatelliteTokenProtector.Protect</c>, with an in-memory Data Protection provider.
/// </summary>
public sealed class ProtectTests
{
    private const string Token = "abcdefghijklmnopqrstuvwxyzABCDEF0123456789_";

    private readonly SatelliteTokenProtectorUnderTest _protector = new(
        new EphemeralDataProtectionProvider(),
        new LoggerFake<SatelliteTokenProtectorUnderTest>());

    [Fact]
    public void ReturnsAValueThatDoesNotContainTheToken()
    {
        string protectedToken = _protector.Protect(Token);

        Assert.DoesNotContain(Token, protectedToken);
    }

    [Fact]
    public void ReturnsAValueThatDecryptsToTheToken()
    {
        string protectedToken = _protector.Protect(Token);

        Assert.True(_protector.TryUnprotect(protectedToken, out string token));
        Assert.Equal(Token, token);
    }
}
