using GwiOS.WebUI.StatusMessages.Contracts.Models;
using GwiOSStatusBarUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSStatusBar;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSStatusBar;

/// <summary>
/// Covers opening and closing the message history of <c>GwiOSStatusBar</c>.
/// </summary>
public sealed class ToggleHistoryTests : StatusBarProbe
{
    [Fact]
    public void OpensTheHistoryWithAllMessagesNewestFirst_WhenTheBarIsClicked()
    {
        StatusMessages.Publish(StatusLevel.Neutral, "Erste");
        StatusMessages.Publish(StatusLevel.Warning, "Zweite");
        IRenderedComponent<GwiOSStatusBarUnderTest> statusBar = RenderStatusBar();

        statusBar.Find(".gwios-status-bar-button").Click();

        Assert.Equal(
            ["Zweite", "Erste"],
            statusBar.FindAll(".gwios-status-history-text").Select(text => text.TextContent));
        Assert.Equal("true", statusBar.Find(".gwios-status-bar-button").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void ShowsAnEmptyHistory_WhenNoMessageWasPublished()
    {
        IRenderedComponent<GwiOSStatusBarUnderTest> statusBar = RenderStatusBar();

        statusBar.Find(".gwios-status-bar-button").Click();

        Assert.Equal("Keine Statusmeldungen.", statusBar.Find(".gwios-status-history .gwios-empty").TextContent);
    }

    [Fact]
    public void ClosesTheHistory_WhenTheBarIsClickedAgain()
    {
        IRenderedComponent<GwiOSStatusBarUnderTest> statusBar = RenderStatusBar();

        statusBar.Find(".gwios-status-bar-button").Click();
        statusBar.Find(".gwios-status-bar-button").Click();

        Assert.Empty(statusBar.FindAll(".gwios-status-history"));
        Assert.Equal("false", statusBar.Find(".gwios-status-bar-button").GetAttribute("aria-expanded"));
    }

    [Fact]
    public void ClosesTheHistory_WhenTheBackdropIsClicked()
    {
        IRenderedComponent<GwiOSStatusBarUnderTest> statusBar = RenderStatusBar();
        statusBar.Find(".gwios-status-bar-button").Click();

        statusBar.Find(".gwios-status-backdrop").Click();

        Assert.Empty(statusBar.FindAll(".gwios-status-history"));
    }

    [Fact]
    public void ClosesTheHistory_WhenTheCloseButtonIsClicked()
    {
        IRenderedComponent<GwiOSStatusBarUnderTest> statusBar = RenderStatusBar();
        statusBar.Find(".gwios-status-bar-button").Click();

        statusBar.Find(".gwios-status-history-close").Click();

        Assert.Empty(statusBar.FindAll(".gwios-status-history"));
    }
}
