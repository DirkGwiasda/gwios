using SatelliteAdministrationUnderTest = GwiOS.WebUI.Components.Pages.Admin.SatelliteAdministration;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.SatelliteAdministration;

/// <summary>
/// Covers closing the token dialog of <c>SatelliteAdministration</c>.
/// </summary>
public sealed class CloseTokenDialogTests : SatelliteAdministrationProbe
{
    [Fact]
    public void RemovesTheTokenFromThePage()
    {
        IRenderedComponent<SatelliteAdministrationUnderTest> page = RenderPage();
        page.Find(".gwios-satellite-administration-header .gwios-button").Click();
        page.Find("#satellite-create-name").Input("keller");
        FindDialogButton(page, "Anlegen").Click();

        FindDialogButton(page, "Schließen").Click();

        Assert.Empty(page.FindAll(".gwios-dialog"));
        Assert.DoesNotContain("token-1", page.Markup);
    }
}
