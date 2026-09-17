using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.SatelliteManager;

/// <summary>
/// Covers <c>SatelliteManager.VerifyTokenAsync</c>.
/// </summary>
public sealed class VerifyTokenAsyncTests
{
    private readonly SatelliteManagerProbe _probe = new();

    [Fact]
    public async Task ReturnsNormally_WhenTheTokenMatches()
    {
        _probe.AddSatellite("keller", token: "right-token");

        await _probe.SatelliteManager.VerifyTokenAsync("keller", "right-token", TestContext.Current.CancellationToken);

        Assert.Empty(_probe.Logger.Entries);
    }

    [Fact]
    public async Task ThrowsSatelliteNotFoundException_WhenNoSuchSatelliteIsStored()
    {
        SatelliteNotFoundException exception = await Assert.ThrowsAsync<SatelliteNotFoundException>(
            () => _probe.SatelliteManager.VerifyTokenAsync("keller", "token", TestContext.Current.CancellationToken));

        Assert.Equal("keller", exception.SatelliteName);
    }

    [Theory]
    [InlineData("wrong-token")]
    [InlineData("right-toke")]
    [InlineData("right-token-and-more")]
    [InlineData("")]
    public async Task ThrowsSatelliteTokenRejectedException_WhenTheTokenDiffers(string token)
    {
        _probe.AddSatellite("keller", token: "right-token");

        SatelliteTokenRejectedException exception = await Assert.ThrowsAsync<SatelliteTokenRejectedException>(
            () => _probe.SatelliteManager.VerifyTokenAsync("keller", token, TestContext.Current.CancellationToken));

        Assert.Equal("keller", exception.SatelliteName);
    }

    [Fact]
    public async Task LogsAWarningWithoutTheToken_WhenTheTokenDiffers()
    {
        _probe.AddSatellite("keller", token: "right-token");

        await Assert.ThrowsAsync<SatelliteTokenRejectedException>(
            () => _probe.SatelliteManager.VerifyTokenAsync("keller", "wrong-token", TestContext.Current.CancellationToken));

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Warning, logEntry.LogLevel);
        Assert.DoesNotContain(logEntry.ContextData.Values, value => value.Contains("token"));
    }

    [Fact]
    public async Task ThrowsSatelliteTokenRejectedException_WhenTheSatelliteIsLocked()
    {
        _probe.AddSatellite("keller", token: "right-token", isLocked: true);

        await Assert.ThrowsAsync<SatelliteTokenRejectedException>(
            () => _probe.SatelliteManager.VerifyTokenAsync("keller", "right-token", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ThrowsSatelliteTokenRejectedException_WhenTheSatelliteHasNoToken()
    {
        _probe.AddSatellite("keller", token: null);

        await Assert.ThrowsAsync<SatelliteTokenRejectedException>(
            () => _probe.SatelliteManager.VerifyTokenAsync("keller", "", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ThrowsSatelliteTokenRejectedException_WhenTheStoredTokenCannotBeDecrypted()
    {
        _probe.AddSatellite("keller", token: "right-token");
        _probe.TokenProtector.CanUnprotect = false;

        await Assert.ThrowsAsync<SatelliteTokenRejectedException>(
            () => _probe.SatelliteManager.VerifyTokenAsync("keller", "right-token", TestContext.Current.CancellationToken));
    }
}
