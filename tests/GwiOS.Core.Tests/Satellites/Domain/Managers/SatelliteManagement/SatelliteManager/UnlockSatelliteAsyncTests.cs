using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.SatelliteManager;

/// <summary>
/// Covers <c>SatelliteManager.UnlockSatelliteAsync</c>.
/// </summary>
public sealed class UnlockSatelliteAsyncTests
{
    private readonly SatelliteManagerProbe _probe = new();

    [Fact]
    public async Task UnlocksTheSatellite()
    {
        _probe.AddSatellite("keller", isLocked: true);

        await _probe.SatelliteManager.UnlockSatelliteAsync("keller", TestContext.Current.CancellationToken);

        Assert.False(_probe.GetStoredSatellite("keller").IsLocked);
    }

    [Fact]
    public async Task KeepsASatelliteWithoutTokenWithoutToken()
    {
        _probe.AddSatellite("fremd", token: null, isLocked: true);

        await _probe.SatelliteManager.UnlockSatelliteAsync("fremd", TestContext.Current.CancellationToken);

        Assert.Null(_probe.GetStoredSatellite("fremd").ProtectedToken);
    }

    [Fact]
    public async Task ThrowsSatelliteNotFoundException_WhenNoSuchSatelliteIsStored()
    {
        await Assert.ThrowsAsync<SatelliteNotFoundException>(
            () => _probe.SatelliteManager.UnlockSatelliteAsync("keller", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LogsTheUnlock()
    {
        _probe.AddSatellite("keller", isLocked: true);

        await _probe.SatelliteManager.UnlockSatelliteAsync("keller", TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal("Satellite unlocked.", logEntry.Message);
    }
}
