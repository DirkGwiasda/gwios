using GwiOS.WebUI.StatusMessages.Contracts.Models;
using SatelliteAdministrationUnderTest = GwiOS.WebUI.Components.Pages.Admin.SatelliteAdministration;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.SatelliteAdministration;

/// <summary>
/// Covers locking and unlocking a satellite on <c>SatelliteAdministration</c>.
/// </summary>
public sealed class ToggleLockAsyncTests : SatelliteAdministrationProbe
{
    [Fact]
    public void LocksAnActiveSatellite()
    {
        AddSatellite("keller");
        IRenderedComponent<SatelliteAdministrationUnderTest> page = RenderPage();

        page.Find(".gwios-satellite-admin-actions .gwios-button--warning").Click();

        Assert.True(SatelliteManager.Satellites.Single().IsLocked);
        Assert.Equal("Gesperrt", page.Find(".gwios-satellite-admin-badges .gwios-badge--danger").TextContent);
        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Warning, message.Level);
        Assert.Equal("Satellit „keller“ gesperrt", message.Text);
    }

    [Fact]
    public void UnlocksALockedSatellite()
    {
        AddSatellite("keller", isLocked: true);
        IRenderedComponent<SatelliteAdministrationUnderTest> page = RenderPage();

        page.Find(".gwios-satellite-admin-actions .gwios-button--success").Click();

        Assert.False(SatelliteManager.Satellites.Single().IsLocked);
        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Success, message.Level);
        Assert.Equal("Satellit „keller“ entsperrt", message.Text);
    }

    [Fact]
    public void PublishesAnError_WhenTheSatelliteWasDeletedMeanwhile()
    {
        AddSatellite("keller");
        IRenderedComponent<SatelliteAdministrationUnderTest> page = RenderPage();
        SatelliteManager.Remove("keller");

        page.Find(".gwios-satellite-admin-actions .gwios-button--warning").Click();

        Assert.Equal("Satellit „keller“ gibt es nicht mehr", Assert.Single(StatusMessages.Messages).Text);
    }
}
