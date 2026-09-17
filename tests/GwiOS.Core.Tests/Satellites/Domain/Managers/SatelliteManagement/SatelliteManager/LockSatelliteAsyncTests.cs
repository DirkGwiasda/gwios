using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.SatelliteManager;

/// <summary>
/// Covers <c>SatelliteManager.LockSatelliteAsync</c>.
/// </summary>
public sealed class LockSatelliteAsyncTests
{
    private readonly SatelliteManagerProbe _probe = new();

    [Fact]
    public async Task LocksTheSatellite()
    {
        _probe.AddSatellite("keller");

        await _probe.SatelliteManager.LockSatelliteAsync("keller", TestContext.Current.CancellationToken);

        Assert.True(_probe.GetStoredSatellite("keller").IsLocked);
    }

    [Fact]
    public async Task ThrowsSatelliteNotFoundException_WhenNoSuchSatelliteIsStored()
    {
        await Assert.ThrowsAsync<SatelliteNotFoundException>(
            () => _probe.SatelliteManager.LockSatelliteAsync("keller", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LogsTheLock()
    {
        _probe.AddSatellite("keller");

        await _probe.SatelliteManager.LockSatelliteAsync("keller", TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal("Satellite locked.", logEntry.Message);
        Assert.Equal("keller", logEntry.ContextData["SatelliteName"]);
    }
}
