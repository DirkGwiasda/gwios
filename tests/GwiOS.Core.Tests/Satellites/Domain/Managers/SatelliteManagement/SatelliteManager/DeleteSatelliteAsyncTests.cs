using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.SatelliteManager;

/// <summary>
/// Covers <c>SatelliteManager.DeleteSatelliteAsync</c>.
/// </summary>
public sealed class DeleteSatelliteAsyncTests
{
    private readonly SatelliteManagerProbe _probe = new();

    [Fact]
    public async Task DeletesOnlyTheSatelliteWithTheGivenName()
    {
        _probe.AddSatellite("keller");
        _probe.AddSatellite("garage");

        await _probe.SatelliteManager.DeleteSatelliteAsync("keller", TestContext.Current.CancellationToken);

        Assert.Equal("garage", Assert.Single(_probe.SatelliteRepository.Satellites).Name);
    }

    [Fact]
    public async Task DoesNothing_WhenNoSuchSatelliteIsStored()
    {
        await _probe.SatelliteManager.DeleteSatelliteAsync("keller", TestContext.Current.CancellationToken);

        Assert.Empty(_probe.SatelliteRepository.Satellites);
    }

    [Fact]
    public async Task LogsTheDeletion()
    {
        _probe.AddSatellite("keller");

        await _probe.SatelliteManager.DeleteSatelliteAsync("keller", TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal("Satellite deleted.", logEntry.Message);
        Assert.Equal("keller", logEntry.ContextData["SatelliteName"]);
    }
}
