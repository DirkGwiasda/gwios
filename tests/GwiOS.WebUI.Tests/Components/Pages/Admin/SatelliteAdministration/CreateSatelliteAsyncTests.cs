using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using SatelliteAdministrationUnderTest = GwiOS.WebUI.Components.Pages.Admin.SatelliteAdministration;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.SatelliteAdministration;

/// <summary>
/// Covers creating a satellite on <c>SatelliteAdministration</c>.
/// </summary>
public sealed class CreateSatelliteAsyncTests : SatelliteAdministrationProbe
{
    [Fact]
    public void CreatesTheSatelliteAndShowsItsTokenOnce()
    {
        IRenderedComponent<SatelliteAdministrationUnderTest> page = RenderPage();

        Create(page, "kinderzimmer-mila", "Kinderzimmer Mila");

        Satellite satellite = Assert.Single(SatelliteManager.Satellites);
        Assert.Equal("kinderzimmer-mila", satellite.Name);
        Assert.Equal("Kinderzimmer Mila", satellite.DisplayName);
        Assert.Equal("Token", page.Find(".gwios-dialog-title").TextContent);
        Assert.Equal("token-1", page.Find(".gwios-satellite-token-value").TextContent);
        Assert.Equal("kinderzimmer-mila", page.Find(".gwios-satellite-admin-name").TextContent);
        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Success, message.Level);
        Assert.Equal("Satellit „kinderzimmer-mila“ angelegt", message.Text);
    }

    [Fact]
    public void TrimsTheNameAndTurnsItToLowercase()
    {
        IRenderedComponent<SatelliteAdministrationUnderTest> page = RenderPage();

        Create(page, "  Keller ", "");

        Assert.Equal("keller", Assert.Single(SatelliteManager.Satellites).Name);
    }

    [Fact]
    public void DisablesAnlegen_WhileTheNameIsEmpty()
    {
        IRenderedComponent<SatelliteAdministrationUnderTest> page = RenderPage();
        page.Find(".gwios-satellite-administration-header .gwios-button").Click();

        Assert.True(FindDialogButton(page, "Anlegen").HasAttribute("disabled"));
    }

    [Fact]
    public void ShowsAnErrorAtTheName_WhenItIsInvalid()
    {
        IRenderedComponent<SatelliteAdministrationUnderTest> page = RenderPage();

        Create(page, "Kinder zimmer", "");

        Assert.Equal("Nur a–z, 0–9 und Bindestrich, 1–32 Zeichen.", page.Find(".gwios-form-field-error").TextContent);
        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Error, message.Level);
        Assert.Equal("Satellit konnte nicht angelegt werden – Name ungültig", message.Text);
        Assert.Empty(SatelliteManager.Satellites);
    }

    [Fact]
    public void ShowsAnErrorAtTheName_WhenItIsAlreadyUsed()
    {
        AddSatellite("keller");
        IRenderedComponent<SatelliteAdministrationUnderTest> page = RenderPage();

        Create(page, "keller", "");

        Assert.Equal("Dieser Name ist bereits vergeben.", page.Find(".gwios-form-field-error").TextContent);
        Assert.Equal("Satellit konnte nicht angelegt werden – Name doppelt", Assert.Single(StatusMessages.Messages).Text);
    }

    [Fact]
    public void ClearsTheNameError_WhenTheNameIsChanged()
    {
        AddSatellite("keller");
        IRenderedComponent<SatelliteAdministrationUnderTest> page = RenderPage();
        Create(page, "keller", "");

        page.Find("#satellite-create-name").Input("garage");

        Assert.Empty(page.FindAll(".gwios-form-field-error"));
    }

    [Fact]
    public void CreatesNothing_WhenTheDialogIsCancelled()
    {
        IRenderedComponent<SatelliteAdministrationUnderTest> page = RenderPage();
        page.Find(".gwios-satellite-administration-header .gwios-button").Click();
        page.Find("#satellite-create-name").Input("keller");

        FindDialogButton(page, "Abbrechen").Click();

        Assert.Empty(page.FindAll(".gwios-dialog"));
        Assert.Empty(SatelliteManager.Satellites);
    }

    private static void Create(IRenderedComponent<SatelliteAdministrationUnderTest> page, string name, string displayName)
    {
        page.Find(".gwios-satellite-administration-header .gwios-button").Click();
        page.Find("#satellite-create-name").Input(name);
        page.Find("#satellite-create-display-name").Input(displayName);
        FindDialogButton(page, "Anlegen").Click();
    }
}
