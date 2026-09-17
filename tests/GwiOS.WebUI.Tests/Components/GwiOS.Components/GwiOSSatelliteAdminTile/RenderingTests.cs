using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;
using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using GwiOSSatelliteAdminTileUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSSatelliteAdminTile;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSSatelliteAdminTile;

/// <summary>
/// Covers how <c>GwiOSSatelliteAdminTile</c> shows a satellite in the administration, using a clock in UTC.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    public RenderingTests()
    {
        Services.AddSingleton<TimeProvider>(new TimeProviderFake());
    }

    [Fact]
    public void ShowsAnActiveSatelliteWithWhatItReportedLast()
    {
        IRenderedComponent<GwiOSSatelliteAdminTileUnderTest> tile = RenderTile(
            new Satellite { Name = "arbeitszimmer", DisplayName = "Arbeitszimmer", ProtectedToken = "protected" },
            new TestSatelliteEntries().CreatePresence("arbeitszimmer"));

        Assert.Equal("arbeitszimmer", tile.Find(".gwios-satellite-admin-name").TextContent);
        Assert.Equal("Arbeitszimmer", tile.Find(".gwios-satellite-admin-display-name").TextContent);
        Assert.Equal("Aktiv", tile.Find(".gwios-badge--success").TextContent);
        Assert.Empty(tile.FindAll(".gwios-badge--muted"));
        Assert.Equal(
            ["14.07.2026 10:29:56", "ESP32 DevKit C", "0.1.0"],
            tile.FindAll(".gwios-satellite-admin-facts dd").Select(fact => fact.TextContent.Trim()));
        Assert.Equal("Sperren", tile.Find(".gwios-button--warning").TextContent.Trim());
    }

    [Fact]
    public void ShowsALockedSatelliteWithoutTokenThatNeverPinged()
    {
        IRenderedComponent<GwiOSSatelliteAdminTileUnderTest> tile = RenderTile(
            new Satellite { Name = "fremd", IsLocked = true },
            presence: null);

        Assert.Contains("gwios-satellite-admin-tile--locked", tile.Find(".gwios-satellite-admin-tile").ClassList);
        Assert.Equal("kein Anzeigename", tile.Find(".gwios-satellite-admin-display-name").TextContent);
        Assert.Equal("Gesperrt", tile.Find(".gwios-badge--danger").TextContent);
        Assert.Equal("kein Token", tile.Find(".gwios-badge--muted").TextContent);
        Assert.Equal(
            ["noch nie", "unbekannt", "—"],
            tile.FindAll(".gwios-satellite-admin-facts dd").Select(fact => fact.TextContent.Trim()));
        Assert.Equal("Entsperren", tile.Find(".gwios-button--success").TextContent.Trim());
    }

    private IRenderedComponent<GwiOSSatelliteAdminTileUnderTest> RenderTile(Satellite satellite, SatellitePresence? presence)
        => Render<GwiOSSatelliteAdminTileUnderTest>(parameters => parameters
            .Add(component => component.Satellite, satellite)
            .Add(component => component.Presence, presence));
}
