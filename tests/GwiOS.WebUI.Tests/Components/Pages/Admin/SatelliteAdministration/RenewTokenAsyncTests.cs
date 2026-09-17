using GwiOS.WebUI.StatusMessages.Contracts.Models;
using SatelliteAdministrationUnderTest = GwiOS.WebUI.Components.Pages.Admin.SatelliteAdministration;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.SatelliteAdministration;

/// <summary>
/// Covers creating a new token for a satellite on <c>SatelliteAdministration</c>.
/// </summary>
public sealed class RenewTokenAsyncTests : SatelliteAdministrationProbe
{
    [Fact]
    public void ShowsTheNewTokenAfterConfirmation()
    {
        AddSatellite("keller");
        IRenderedComponent<SatelliteAdministrationUnderTest> page = RenderPage();

        Renew(page);

        Assert.Equal("token-1", page.Find(".gwios-satellite-token-value").TextContent);
        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Warning, message.Level);
        Assert.Equal("Neues Token für „keller“ erzeugt", message.Text);
    }

    [Fact]
    public void PublishesAnErrorAndShowsNoToken_WhenTheSatelliteWasDeletedMeanwhile()
    {
        AddSatellite("keller");
        IRenderedComponent<SatelliteAdministrationUnderTest> page = RenderPage();
        SatelliteManager.Remove("keller");

        Renew(page);

        Assert.Empty(page.FindAll(".gwios-dialog"));
        Assert.Equal(StatusLevel.Error, Assert.Single(StatusMessages.Messages).Level);
    }

    private static void Renew(IRenderedComponent<SatelliteAdministrationUnderTest> page)
    {
        page.Find("button[title='Neues Token für „keller“ erzeugen']").Click();
        page.Find(".gwios-satellite-admin-actions .gwios-button--warning[aria-label^='Zum Bestätigen']").Click();
    }
}
