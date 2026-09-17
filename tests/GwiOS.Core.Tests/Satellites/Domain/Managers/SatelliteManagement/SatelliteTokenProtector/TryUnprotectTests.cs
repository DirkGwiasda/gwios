using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using Microsoft.AspNetCore.DataProtection;
using SatelliteTokenProtectorUnderTest = GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.SatelliteTokenProtector;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.SatelliteTokenProtector;

/// <summary>
/// Covers <c>SatelliteTokenProtector.TryUnprotect</c>, with in-memory Data Protection providers.
/// </summary>
public sealed class TryUnprotectTests
{
    private readonly LoggerFake<SatelliteTokenProtectorUnderTest> _logger = new();
    private readonly SatelliteTokenProtectorUnderTest _protector;

    public TryUnprotectTests()
    {
        _protector = new SatelliteTokenProtectorUnderTest(new EphemeralDataProtectionProvider(), _logger);
    }

    [Fact]
    public void ReturnsTheToken_WhenItWasProtectedWithTheSameKeys()
    {
        string protectedToken = _protector.Protect("my-token");

        bool isUnprotected = _protector.TryUnprotect(protectedToken, out string token);

        Assert.True(isUnprotected);
        Assert.Equal("my-token", token);
        Assert.Empty(_logger.Entries);
    }

    [Fact]
    public void ReturnsFalseAndLogsAnError_WhenTheKeysAreLost()
    {
        SatelliteTokenProtectorUnderTest otherProtector = new(
            new EphemeralDataProtectionProvider(),
            new LoggerFake<SatelliteTokenProtectorUnderTest>());
        string protectedToken = otherProtector.Protect("my-token");

        bool isUnprotected = _protector.TryUnprotect(protectedToken, out string token);

        Assert.False(isUnprotected);
        Assert.Equal(string.Empty, token);
        Assert.Equal(LogLevel.Error, Assert.Single(_logger.Entries).LogLevel);
    }

    [Fact]
    public void ReturnsFalse_WhenTheValueIsNoProtectedToken()
    {
        Assert.False(_protector.TryUnprotect("my-token", out _));
    }
}
