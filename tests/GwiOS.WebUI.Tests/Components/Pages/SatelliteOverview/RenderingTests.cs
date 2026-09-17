using SatelliteOverviewUnderTest = GwiOS.WebUI.Components.Pages.SatelliteOverview;

namespace GwiOS.WebUI.Tests.Components.Pages.SatelliteOverview;

/// <summary>
/// Covers what <c>SatelliteOverview</c> shows.
/// </summary>
public sealed class RenderingTests : SatelliteOverviewProbe
{
    [Fact]
    public void ShowsATileForEverySatellite()
    {
        AddEntry(Entries.CreateOnline("arbeitszimmer", "Arbeitszimmer"));
        AddEntry(Entries.CreateOffline("garage"));

        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();

        Assert.Equal(["Arbeitszimmer", "garage"], page.FindAll(".gwios-satellite-title").Select(title => title.TextContent));
    }

    [Fact]
    public void CountsOnlineAndOfflineSatellites()
    {
        AddEntry(Entries.CreateOnline("arbeitszimmer"));
        AddEntry(Entries.CreateOffline("garage"));
        AddSatellite("neu");

        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();

        Assert.Equal("1/3 Satelliten aktiv · 2 offline", page.Find(".gwios-satellites-network").TextContent.Trim());
        Assert.Contains("gwios-badge--danger", page.Find(".gwios-satellites-network").ClassList);
    }

    [Fact]
    public void ShowsTheNetworkInGreen_WhenAllSatellitesAreOnline()
    {
        AddEntry(Entries.CreateOnline("arbeitszimmer"));

        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();

        Assert.Equal("1/1 Satelliten aktiv", page.Find(".gwios-satellites-network").TextContent.Trim());
        Assert.Contains("gwios-badge--success", page.Find(".gwios-satellites-network").ClassList);
    }

    [Fact]
    public void ShowsTheTimeOfTheLastFetch()
    {
        OverviewWorkflow.LastFetchedAt = new DateTime(2026, 7, 14, 10, 29, 45, DateTimeKind.Utc);

        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();

        Assert.Equal("Letzte Abfrage 10:29:45", page.Find(".gwios-satellites-last-poll").TextContent);
    }

    [Fact]
    public void ShowsADash_WhenNothingWasFetchedYet()
    {
        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();

        Assert.Equal("Letzte Abfrage –", page.Find(".gwios-satellites-last-poll").TextContent);
    }

    [Fact]
    public void SaysSo_WhenThereAreNoSatellites()
    {
        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();

        Assert.Equal("Noch keine Satelliten angelegt.", page.Find(".gwios-empty").TextContent);
    }

    [Fact]
    public void ShowsTheUnknownSatellitesOnlyWhenThereAreAny()
    {
        IRenderedComponent<SatelliteOverviewUnderTest> pageWithoutCandidates = RenderPage();
        Assert.Empty(pageWithoutCandidates.FindAll(".gwios-satellite-candidate-tile"));

        OverviewWorkflow.Candidates.Add(Entries.CreateCandidate("garage"));
        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();

        Assert.Equal("Unbekannte Satelliten", page.Find(".gwios-section-title").TextContent);
        Assert.Equal("garage", page.Find(".gwios-satellite-candidate-name").TextContent);
    }

    [Fact]
    public void StartsWithAutomaticRefreshSwitchedOn()
    {
        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();

        Assert.True(page.Find(".gwios-satellites-auto-refresh input").HasAttribute("checked"));
    }
}
