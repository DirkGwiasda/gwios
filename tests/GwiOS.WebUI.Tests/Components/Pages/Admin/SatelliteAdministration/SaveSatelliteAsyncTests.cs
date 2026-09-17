using GwiOS.WebUI.StatusMessages.Contracts.Models;
using SatelliteAdministrationUnderTest = GwiOS.WebUI.Components.Pages.Admin.SatelliteAdministration;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.SatelliteAdministration;

/// <summary>
/// Covers editing the display name of a satellite on <c>SatelliteAdministration</c>.
/// </summary>
public sealed class SaveSatelliteAsyncTests : SatelliteAdministrationProbe
{
    [Fact]
    public void OpensTheDialogWithTheFixedNameAndTheCurrentDisplayName()
    {
        AddSatellite("keller", "Keller");
        IRenderedComponent<SatelliteAdministrationUnderTest> page = RenderPage();

        page.Find("button[title='„keller“ bearbeiten']").Click();

        Assert.Equal("Satellit bearbeiten", page.Find(".gwios-dialog-title").TextContent);
        Assert.Equal("keller", page.Find(".gwios-read-only-field-value").TextContent);
        Assert.Equal("Keller", page.Find("#satellite-edit-display-name").GetAttribute("value"));
    }

    [Fact]
    public void StoresTheDisplayNameAndClosesTheDialog()
    {
        AddSatellite("keller", "Keller");
        IRenderedComponent<SatelliteAdministrationUnderTest> page = RenderPage();
        page.Find("button[title='„keller“ bearbeiten']").Click();

        page.Find("#satellite-edit-display-name").Input("Vorratskeller");
        FindDialogButton(page, "Speichern").Click();

        Assert.Equal("Vorratskeller", SatelliteManager.Satellites.Single().DisplayName);
        Assert.Empty(page.FindAll(".gwios-dialog"));
        Assert.Equal("Vorratskeller", page.Find(".gwios-satellite-admin-display-name").TextContent);
        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Success, message.Level);
        Assert.Equal("Satellit „keller“ gespeichert", message.Text);
    }

    [Fact]
    public void ClosesTheDialogWithAnError_WhenTheSatelliteWasDeletedMeanwhile()
    {
        AddSatellite("keller");
        IRenderedComponent<SatelliteAdministrationUnderTest> page = RenderPage();
        page.Find("button[title='„keller“ bearbeiten']").Click();
        SatelliteManager.Remove("keller");

        FindDialogButton(page, "Speichern").Click();

        Assert.Empty(page.FindAll(".gwios-dialog"));
        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Error, message.Level);
        Assert.Equal("Satellit „keller“ gibt es nicht mehr", message.Text);
        Assert.Empty(page.FindAll(".gwios-satellite-admin-tile"));
    }
}
