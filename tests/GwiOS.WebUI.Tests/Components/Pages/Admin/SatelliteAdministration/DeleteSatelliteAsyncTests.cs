using GwiOS.WebUI.StatusMessages.Contracts.Models;
using SatelliteAdministrationUnderTest = GwiOS.WebUI.Components.Pages.Admin.SatelliteAdministration;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.SatelliteAdministration;

/// <summary>
/// Covers deleting a satellite on <c>SatelliteAdministration</c>.
/// </summary>
public sealed class DeleteSatelliteAsyncTests : SatelliteAdministrationProbe
{
    [Fact]
    public void DeletesOnlyTheSatelliteWhoseDeletionIsConfirmed()
    {
        AddSatellite("garage");
        AddSatellite("keller");
        IRenderedComponent<SatelliteAdministrationUnderTest> page = RenderPage();

        page.Find("button[title='„keller“ löschen']").Click();
        page.Find(".gwios-satellite-admin-actions .gwios-button--danger").Click();

        Assert.Equal("garage", Assert.Single(SatelliteManager.Satellites).Name);
        Assert.Equal(["garage"], page.FindAll(".gwios-satellite-admin-name").Select(name => name.TextContent));
        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Warning, message.Level);
        Assert.Equal("Satellit „keller“ gelöscht", message.Text);
    }
}
