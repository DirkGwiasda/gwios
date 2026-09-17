using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.Contracts;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.SatelliteManager;

/// <summary>
/// Covers <c>SatelliteManager.RenewTokenAsync</c>.
/// </summary>
public sealed class RenewTokenAsyncTests
{
    private readonly SatelliteManagerProbe _probe = new();

    [Fact]
    public async Task ReturnsTheNewTokenAndStoresItEncrypted()
    {
        _probe.AddSatellite("keller", token: "old-token");

        string token = await _probe.SatelliteManager.RenewTokenAsync("keller", TestContext.Current.CancellationToken);

        Assert.Equal("generated-token-1", token);
        Assert.Equal(SatelliteTokenProtectorFake.Prefix + "generated-token-1", _probe.GetStoredSatellite("keller").ProtectedToken);
    }

    [Fact]
    public async Task GivesASatelliteWithoutTokenAToken()
    {
        _probe.AddSatellite("fremd", token: null);

        await _probe.SatelliteManager.RenewTokenAsync("fremd", TestContext.Current.CancellationToken);

        Assert.NotNull(_probe.GetStoredSatellite("fremd").ProtectedToken);
    }

    [Fact]
    public async Task ThrowsSatelliteNotFoundException_WhenNoSuchSatelliteIsStored()
    {
        await Assert.ThrowsAsync<SatelliteNotFoundException>(
            () => _probe.SatelliteManager.RenewTokenAsync("keller", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LogsTheRenewal()
    {
        _probe.AddSatellite("keller");

        await _probe.SatelliteManager.RenewTokenAsync("keller", TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal("Satellite token renewed.", logEntry.Message);
    }
}
