using SatelliteOverviewUnderTest = GwiOS.WebUI.Components.Pages.SatelliteOverview;

namespace GwiOS.WebUI.Tests.Components.Pages.SatelliteOverview;

/// <summary>
/// Covers the automatic refresh of <c>SatelliteOverview</c> and switching it off and on.
/// </summary>
public sealed class ChangeAutoRefreshAsyncTests : SatelliteOverviewProbe
{
    [Fact]
    public void ReadsTheOverviewAgainEveryFiveSeconds()
    {
        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();
        AddEntry(Entries.CreateOnline("arbeitszimmer"));

        TimeProvider.Advance(TimeSpan.FromSeconds(5));

        page.WaitForAssertion(() => Assert.Single(page.FindAll(".gwios-satellite-tile")));
        Assert.Equal(2, OverviewWorkflow.CallCount);
        TimeProvider.Advance(TimeSpan.FromSeconds(5));
        page.WaitForAssertion(() => Assert.Equal(3, OverviewWorkflow.CallCount));
    }

    [Fact]
    public void StopsReading_WhenTheCheckboxIsCleared()
    {
        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();

        page.Find(".gwios-satellites-auto-refresh input").Change(false);
        TimeProvider.Advance(TimeSpan.FromSeconds(30));

        Assert.Equal(1, OverviewWorkflow.CallCount);
        Assert.False(page.Find(".gwios-satellites-auto-refresh input").HasAttribute("checked"));
    }

    [Fact]
    public void ReadsRightAwayAndThenEveryFiveSecondsAgain_WhenTheCheckboxIsSetAgain()
    {
        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();
        page.Find(".gwios-satellites-auto-refresh input").Change(false);

        page.Find(".gwios-satellites-auto-refresh input").Change(true);
        Assert.Equal(2, OverviewWorkflow.CallCount);
        TimeProvider.Advance(TimeSpan.FromSeconds(5));

        page.WaitForAssertion(() => Assert.Equal(3, OverviewWorkflow.CallCount));
    }

    [Fact]
    public async Task StopsReading_WhenThePageIsDisposed()
    {
        RenderPage();

        await DisposeComponentsAsync();
        TimeProvider.Advance(TimeSpan.FromSeconds(30));

        Assert.Equal(1, OverviewWorkflow.CallCount);
    }
}
