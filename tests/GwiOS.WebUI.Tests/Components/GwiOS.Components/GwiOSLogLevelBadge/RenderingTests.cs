using AngleSharp.Dom;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOSLogLevelBadgeUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSLogLevelBadge;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSLogLevelBadge;

/// <summary>
/// Covers how <c>GwiOSLogLevelBadge</c> labels, colors and sizes each log level.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Theory]
    [InlineData(LogLevel.Error, "Error", "gwios-log-level--error")]
    [InlineData(LogLevel.Warning, "Warning", "gwios-log-level--warning")]
    [InlineData(LogLevel.Information, "Information", "gwios-log-level--information")]
    [InlineData(LogLevel.Debug, "Debug", "gwios-log-level--debug")]
    [InlineData(LogLevel.None, "None", "gwios-log-level--debug")]
    public void ShowsTheLabelAndTheColorOfTheLevel(LogLevel level, string expectedLabel, string expectedClass)
    {
        IRenderedComponent<GwiOSLogLevelBadgeUnderTest> badge = Render<GwiOSLogLevelBadgeUnderTest>(parameters => parameters
            .Add(component => component.Level, level));

        IElement span = badge.Find("span");
        Assert.Equal(expectedLabel, span.TextContent);
        Assert.True(span.ClassList.Contains(expectedClass));
    }

    [Fact]
    public void ShowsTheRegularSize_ByDefault()
    {
        IRenderedComponent<GwiOSLogLevelBadgeUnderTest> badge = Render<GwiOSLogLevelBadgeUnderTest>(parameters => parameters
            .Add(component => component.Level, LogLevel.Information));

        Assert.False(badge.Find("span").ClassList.Contains("gwios-log-level--large"));
    }

    [Fact]
    public void ShowsTheLargeSize_WhenRequested()
    {
        IRenderedComponent<GwiOSLogLevelBadgeUnderTest> badge = Render<GwiOSLogLevelBadgeUnderTest>(parameters => parameters
            .Add(component => component.Level, LogLevel.Information)
            .Add(component => component.IsLarge, true));

        Assert.True(badge.Find("span").ClassList.Contains("gwios-log-level--large"));
    }
}
