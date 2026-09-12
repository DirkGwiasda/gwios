using AngleSharp.Dom;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOSLogLevelBadgeUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSLogLevelBadge;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSLogLevelBadge;

/// <summary>
/// Covers how <c>GwiOSLogLevelBadge</c> labels and colors each log level.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Theory]
    [InlineData(LogLevel.Error, "ERROR", "gwios-log-level--error")]
    [InlineData(LogLevel.Warning, "WARN", "gwios-log-level--warning")]
    [InlineData(LogLevel.Information, "INFO", "gwios-log-level--information")]
    [InlineData(LogLevel.Debug, "DEBUG", "gwios-log-level--debug")]
    [InlineData(LogLevel.None, "NONE", "gwios-log-level--debug")]
    public void ShowsTheLabelAndTheColorOfTheLevel(LogLevel level, string expectedLabel, string expectedClass)
    {
        IRenderedComponent<GwiOSLogLevelBadgeUnderTest> badge = Render<GwiOSLogLevelBadgeUnderTest>(parameters => parameters
            .Add(component => component.Level, level));

        IElement span = badge.Find("span");
        Assert.Equal(expectedLabel, span.TextContent);
        Assert.True(span.ClassList.Contains(expectedClass));
    }
}
