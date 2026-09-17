using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOSSatelliteAdminTileUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSSatelliteAdminTile;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSSatelliteAdminTile;

/// <summary>
/// Covers how <c>GwiOSSatelliteAdminTile</c> reports clicks on its buttons.
/// </summary>
public sealed class ClickTests : BunitContext
{
    private readonly List<string> _events = [];

    [Fact]
    public void RaisesOnEdit_WhenBearbeitenIsClicked()
    {
        IRenderedComponent<GwiOSSatelliteAdminTileUnderTest> tile = RenderTile();

        tile.Find("button[title='„keller“ bearbeiten']").Click();

        Assert.Equal(["edit"], _events);
    }

    [Fact]
    public void RaisesOnToggleLock_WhenSperrenIsClicked()
    {
        IRenderedComponent<GwiOSSatelliteAdminTileUnderTest> tile = RenderTile();

        tile.Find(".gwios-button--warning").Click();

        Assert.Equal(["lock"], _events);
    }

    [Fact]
    public void RaisesOnRenewTokenOnlyAfterTheSecondClick()
    {
        IRenderedComponent<GwiOSSatelliteAdminTileUnderTest> tile = RenderTile();

        tile.Find("button[title='Neues Token für „keller“ erzeugen']").Click();
        Assert.Empty(_events);
        Assert.Equal("Altes Token ungültig?", tile.Find(".gwios-button--warning[aria-label^='Zum Bestätigen']").TextContent.Trim());
        tile.Find(".gwios-button--warning[aria-label^='Zum Bestätigen']").Click();

        Assert.Equal(["renew"], _events);
    }

    [Fact]
    public void RaisesOnDeleteOnlyAfterTheSecondClick()
    {
        IRenderedComponent<GwiOSSatelliteAdminTileUnderTest> tile = RenderTile();

        tile.Find("button[title='„keller“ löschen']").Click();
        Assert.Empty(_events);
        tile.Find(".gwios-button--danger").Click();

        Assert.Equal(["delete"], _events);
    }

    private IRenderedComponent<GwiOSSatelliteAdminTileUnderTest> RenderTile()
        => Render<GwiOSSatelliteAdminTileUnderTest>(parameters => parameters
            .Add(component => component.Satellite, new Satellite { Name = "keller", ProtectedToken = "protected" })
            .Add(component => component.OnEdit, () => _events.Add("edit"))
            .Add(component => component.OnToggleLock, () => _events.Add("lock"))
            .Add(component => component.OnRenewToken, () => _events.Add("renew"))
            .Add(component => component.OnDelete, () => _events.Add("delete")));
}
