using GwiOSTimeAgoUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSTimeAgo;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSTimeAgo;

/// <summary>
/// Covers how <c>GwiOSTimeAgo</c> describes the age of a point in time.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    private static readonly DateTime Now = new(2026, 7, 14, 10, 30, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(0, "vor 0 s")]
    [InlineData(4, "vor 4 s")]
    [InlineData(59, "vor 59 s")]
    [InlineData(60, "vor 1 min")]
    [InlineData(372, "vor 6 min")]
    [InlineData(3599, "vor 59 min")]
    [InlineData(3600, "vor 1 h")]
    [InlineData(86399, "vor 23 h")]
    [InlineData(172800, "vor 2 d")]
    public void DescribesTheAgeInTheLargestWholeUnit(int seconds, string expectedText)
    {
        IRenderedComponent<GwiOSTimeAgoUnderTest> timeAgo = Render<GwiOSTimeAgoUnderTest>(parameters => parameters
            .Add(component => component.Value, Now.AddSeconds(-seconds))
            .Add(component => component.Now, Now));

        Assert.Equal(expectedText, timeAgo.Find(".gwios-time-ago").TextContent);
    }

    [Fact]
    public void PutsThePrefixInFront()
    {
        IRenderedComponent<GwiOSTimeAgoUnderTest> timeAgo = Render<GwiOSTimeAgoUnderTest>(parameters => parameters
            .Add(component => component.Value, Now.AddMinutes(-6))
            .Add(component => component.Now, Now)
            .Add(component => component.Prefix, "zuletzt "));

        Assert.Equal("zuletzt vor 6 min", timeAgo.Find(".gwios-time-ago").TextContent);
    }

    [Fact]
    public void ShowsZeroSeconds_ForAPointInTheFuture()
    {
        IRenderedComponent<GwiOSTimeAgoUnderTest> timeAgo = Render<GwiOSTimeAgoUnderTest>(parameters => parameters
            .Add(component => component.Value, Now.AddSeconds(5))
            .Add(component => component.Now, Now));

        Assert.Equal("vor 0 s", timeAgo.Find(".gwios-time-ago").TextContent);
    }
}
