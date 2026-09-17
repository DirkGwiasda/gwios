using AngleSharp.Dom;
using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using GwiOSTimestampUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSTimestamp;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSTimestamp;

/// <summary>
/// Covers how <c>GwiOSTimestamp</c> shows a point in time, using a clock whose local time zone is two hours ahead of
/// UTC.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    public RenderingTests()
    {
        TimeProviderFake timeProvider = new();
        timeProvider.UseLocalOffset(TimeSpan.FromHours(2));
        Services.AddSingleton<TimeProvider>(timeProvider);
    }

    [Fact]
    public void ShowsTheLocalDateAndTimeToTheSecond()
    {
        IRenderedComponent<GwiOSTimestampUnderTest> timestamp = RenderTimestamp(
            new DateTimeOffset(2026, 7, 14, 23, 5, 9, TimeSpan.Zero));

        Assert.Equal("15.07.2026 01:05:09", timestamp.Find("time").TextContent);
    }

    [Fact]
    public void ShowsTheSameInstant_WhenTheValueHasAnotherOffset()
    {
        IRenderedComponent<GwiOSTimestampUnderTest> timestamp = RenderTimestamp(
            new DateTimeOffset(2026, 7, 14, 10, 0, 0, TimeSpan.FromHours(-5)));

        Assert.Equal("14.07.2026 17:00:00", timestamp.Find("time").TextContent);
    }

    [Fact]
    public void GivesTheMachineReadableValueInTheDatetimeAttribute()
    {
        IRenderedComponent<GwiOSTimestampUnderTest> timestamp = RenderTimestamp(
            new DateTimeOffset(2026, 7, 14, 23, 5, 9, TimeSpan.Zero));

        IElement time = timestamp.Find("time");
        Assert.Equal("2026-07-14T23:05:09.0000000+00:00", time.GetAttribute("datetime"));
    }

    private IRenderedComponent<GwiOSTimestampUnderTest> RenderTimestamp(DateTimeOffset value)
        => Render<GwiOSTimestampUnderTest>(parameters => parameters.Add(component => component.Value, value));
}
