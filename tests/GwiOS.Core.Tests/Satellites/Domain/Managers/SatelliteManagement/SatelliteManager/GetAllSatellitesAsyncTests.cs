using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.SatelliteManager;

/// <summary>
/// Covers <c>SatelliteManager.GetAllSatellitesAsync</c>.
/// </summary>
public sealed class GetAllSatellitesAsyncTests
{
    private readonly SatelliteManagerProbe _probe = new();

    [Fact]
    public async Task ReturnsAllStoredSatellitesOrderedByName()
    {
        _probe.AddSatellite("wohnzimmer");
        _probe.AddSatellite("keller");

        List<Satellite> satellites =
            await _probe.SatelliteManager.GetAllSatellitesAsync(TestContext.Current.CancellationToken);

        Assert.Equal(["keller", "wohnzimmer"], satellites.Select(satellite => satellite.Name));
    }

    [Fact]
    public async Task ReturnsAnEmptyList_WhenNoSatellitesAreStored()
    {
        Assert.Empty(await _probe.SatelliteManager.GetAllSatellitesAsync(TestContext.Current.CancellationToken));
    }
}
