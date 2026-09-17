using GwiOS.WebUI.Tests.TestInfrastructure;
using SatelliteAdministrationUnderTest = GwiOS.WebUI.Components.Pages.Admin.SatelliteAdministration;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.SatelliteAdministration;

/// <summary>
/// Covers what <c>SatelliteAdministration</c> shows.
/// </summary>
public sealed class RenderingTests : SatelliteAdministrationProbe
{
    [Fact]
    public void ShowsATileForEverySatelliteOrderedByName()
    {
        AddSatellite("wohnzimmer");
        AddSatellite("arbeitszimmer", "Arbeitszimmer");

        IRenderedComponent<SatelliteAdministrationUnderTest> page = RenderPage();

        Assert.Equal(["arbeitszimmer", "wohnzimmer"], page.FindAll(".gwios-satellite-admin-name").Select(name => name.TextContent));
    }

    [Fact]
    public void ShowsWhatTheSatelliteReportedLast()
    {
        AddSatellite("arbeitszimmer");
        OverviewWorkflow.Entries["arbeitszimmer"] = new TestSatelliteEntries().CreateOnline("arbeitszimmer");

        IRenderedComponent<SatelliteAdministrationUnderTest> page = RenderPage();

        Assert.Contains("ESP32 DevKit C", page.FindAll(".gwios-satellite-admin-facts dd").Select(fact => fact.TextContent.Trim()));
    }

    [Fact]
    public void SaysSo_WhenThereAreNoSatellites()
    {
        IRenderedComponent<SatelliteAdministrationUnderTest> page = RenderPage();

        Assert.Equal("Noch keine Satelliten angelegt.", page.Find(".gwios-empty").TextContent);
        Assert.Empty(page.FindAll(".gwios-dialog"));
    }
}
