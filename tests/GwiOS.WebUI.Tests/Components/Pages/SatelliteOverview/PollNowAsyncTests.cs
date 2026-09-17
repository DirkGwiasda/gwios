using GwiOS.WebUI.StatusMessages.Contracts.Models;
using SatelliteOverviewUnderTest = GwiOS.WebUI.Components.Pages.SatelliteOverview;

namespace GwiOS.WebUI.Tests.Components.Pages.SatelliteOverview;

/// <summary>
/// Covers "Jetzt abfragen" on <c>SatelliteOverview</c>.
/// </summary>
public sealed class PollNowAsyncTests : SatelliteOverviewProbe
{
    [Fact]
    public void PollsTheSatellitesAndReadsTheOverviewAgain()
    {
        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();

        page.Find(".gwios-satellites-toolbar .gwios-button").Click();

        Assert.Equal(1, PollingWorkflow.PollCount);
        Assert.Equal(2, OverviewWorkflow.CallCount);
        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Neutral, message.Level);
        Assert.Equal("Satelliten werden abgefragt", message.Text);
    }

    [Fact]
    public void DisablesTheButtonWhilePolling()
    {
        TaskCompletionSource release = new();
        PollingWorkflow.PollReleased = release.Task;
        IRenderedComponent<SatelliteOverviewUnderTest> page = RenderPage();

        page.Find(".gwios-satellites-toolbar .gwios-button").Click();

        Assert.True(page.Find(".gwios-satellites-toolbar .gwios-button").HasAttribute("disabled"));
        Assert.Equal("Abfrage …", page.Find(".gwios-satellites-toolbar .gwios-button").TextContent.Trim());
        release.SetResult();
        page.WaitForAssertion(() => Assert.Equal("Jetzt abfragen", page.Find(".gwios-satellites-toolbar .gwios-button").TextContent.Trim()));
    }
}
