using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using GwiOSSatelliteCandidateTileUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSSatelliteCandidateTile;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSSatelliteCandidateTile;

/// <summary>
/// Covers how <c>GwiOSSatelliteCandidateTile</c> shows an unknown satellite, using a clock in UTC.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    private readonly TestSatelliteEntries _entries = new();

    public RenderingTests()
    {
        Services.AddSingleton<TimeProvider>(new TimeProviderFake());
    }

    [Fact]
    public void ShowsWhatTheSatelliteReported()
    {
        IRenderedComponent<GwiOSSatelliteCandidateTileUnderTest> tile = RenderTile(hasTokenChanged: false);

        Assert.Equal("garage", tile.Find(".gwios-satellite-candidate-name").TextContent);
        Assert.Equal("ESP32 DevKit C · Firmware 0.1.0", tile.Find(".gwios-satellite-candidate-board").TextContent);
        Assert.Equal("Unbekannt", tile.Find(".gwios-badge--warning").TextContent);
        Assert.Equal(
            ["192.168.178.40", "-75 dBm", "14.07.2026 10:25:00", "vor 10 s"],
            tile.FindAll(".gwios-satellite-candidate-facts dd").Select(fact => fact.TextContent.Trim()));
        Assert.False(tile.Find("button[title='„garage“ mit seinem Token anlegen']").HasAttribute("disabled"));
    }

    [Fact]
    public void WarnsAndDisablesAdopting_WhenTheTokenChanged()
    {
        IRenderedComponent<GwiOSSatelliteCandidateTileUnderTest> tile = RenderTile(hasTokenChanged: true);

        Assert.Equal("Token wechselt", tile.Find(".gwios-badge--danger").TextContent);
        Assert.Single(tile.FindAll(".gwios-satellite-candidate-warning"));
        Assert.True(tile.Find("button[title='„garage“ mit seinem Token anlegen']").HasAttribute("disabled"));
    }

    private IRenderedComponent<GwiOSSatelliteCandidateTileUnderTest> RenderTile(bool hasTokenChanged)
        => Render<GwiOSSatelliteCandidateTileUnderTest>(parameters => parameters
            .Add(component => component.Candidate, _entries.CreateCandidate("garage", hasTokenChanged))
            .Add(component => component.Now, TestSatelliteEntries.Now));
}
