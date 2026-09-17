using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.SatelliteManager;

/// <summary>
/// Covers <c>SatelliteManager.GetTokenAsync</c>.
/// </summary>
public sealed class GetTokenAsyncTests
{
    private readonly SatelliteManagerProbe _probe = new();

    [Fact]
    public async Task ReturnsTheDecryptedToken()
    {
        _probe.AddSatellite("keller", token: "stored-token");

        string token = await _probe.SatelliteManager.GetTokenAsync("keller", TestContext.Current.CancellationToken);

        Assert.Equal("stored-token", token);
    }

    [Fact]
    public async Task ThrowsSatelliteNotFoundException_WhenNoSuchSatelliteIsStored()
    {
        await Assert.ThrowsAsync<SatelliteNotFoundException>(
            () => _probe.SatelliteManager.GetTokenAsync("keller", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ThrowsSatelliteTokenRejectedException_WhenTheSatelliteIsLocked()
    {
        _probe.AddSatellite("keller", isLocked: true);

        await Assert.ThrowsAsync<SatelliteTokenRejectedException>(
            () => _probe.SatelliteManager.GetTokenAsync("keller", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ThrowsSatelliteTokenRejectedException_WhenTheSatelliteHasNoToken()
    {
        _probe.AddSatellite("keller", token: null);

        await Assert.ThrowsAsync<SatelliteTokenRejectedException>(
            () => _probe.SatelliteManager.GetTokenAsync("keller", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ThrowsSatelliteTokenRejectedException_WhenTheTokenCannotBeDecrypted()
    {
        _probe.AddSatellite("keller");
        _probe.TokenProtector.CanUnprotect = false;

        await Assert.ThrowsAsync<SatelliteTokenRejectedException>(
            () => _probe.SatelliteManager.GetTokenAsync("keller", TestContext.Current.CancellationToken));
    }
}
