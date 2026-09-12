using GwiOS.WebUI.StatusMessages.Contracts.Models;
using GwiOSStatusBarUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSStatusBar;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSStatusBar;

/// <summary>
/// Covers how <c>GwiOSStatusBar</c> renders the current status message.
/// </summary>
public sealed class RenderingTests : StatusBarProbe
{
    [Fact]
    public void ShowsAPlaceholder_WhenNoMessageWasPublished()
    {
        IRenderedComponent<GwiOSStatusBarUnderTest> statusBar = RenderStatusBar();

        Assert.Equal("Keine Statusmeldungen", statusBar.Find(".gwios-status-text").TextContent);
        Assert.Empty(statusBar.FindAll(".gwios-status-history"));
    }

    [Fact]
    public void ShowsTheMostRecentMessageWithItsLocalTime()
    {
        StatusMessages.Publish(StatusLevel.Neutral, "Erste");
        StatusMessages.Publish(StatusLevel.Success, "Zweite");

        IRenderedComponent<GwiOSStatusBarUnderTest> statusBar = RenderStatusBar();

        Assert.Equal("Zweite", statusBar.Find(".gwios-status-text").TextContent);
        Assert.Equal("12:30", statusBar.Find(".gwios-status-bar-time").TextContent);
    }

    [Theory]
    [InlineData(StatusLevel.Neutral, "gwios-status-dot--neutral")]
    [InlineData(StatusLevel.Success, "gwios-status-dot--success")]
    [InlineData(StatusLevel.Warning, "gwios-status-dot--warning")]
    [InlineData(StatusLevel.Error, "gwios-status-dot--error")]
    public void ColorsTheDotWithTheLevelOfTheMessage(StatusLevel level, string expectedClass)
    {
        StatusMessages.Publish(level, "Meldung");

        IRenderedComponent<GwiOSStatusBarUnderTest> statusBar = RenderStatusBar();

        Assert.True(statusBar.Find(".gwios-status-bar .gwios-status-dot").ClassList.Contains(expectedClass));
    }
}
