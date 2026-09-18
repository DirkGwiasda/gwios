using GwiOS.WebUI.Components.GwiOS.Components;
using GwiOSSparkLineUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSSparkLine;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSSparkLine;

/// <summary>
/// Covers how <c>GwiOSSparkLine</c> turns values into a line.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Fact]
    public void SpreadsThePointsOverTheWidthAndTurnsTheHighestValueUpwards()
    {
        IRenderedComponent<GwiOSSparkLineUnderTest> line = RenderLine([10.0, 11, 12], minimumSpan: 1);

        Assert.Equal("0,22.5 50,12 100,1.5", line.Find("polyline").GetAttribute("points"));
    }

    [Fact]
    public void DrawsAFlatLineInTheMiddle_WhenAllValuesAreEqual()
    {
        IRenderedComponent<GwiOSSparkLineUnderTest> line = RenderLine([1000.0, 1000, 1000], minimumSpan: 2);

        Assert.Equal("0,12 50,12 100,12", line.Find("polyline").GetAttribute("points"));
    }

    [Fact]
    public void KeepsAQuietSeriesFlat_WhenItIsNarrowerThanTheMinimumSpan()
    {
        IRenderedComponent<GwiOSSparkLineUnderTest> line = RenderLine([1000.0, 1000.2], minimumSpan: 2);

        Assert.Equal("0,13.05 100,10.95", line.Find("polyline").GetAttribute("points"));
    }

    [Fact]
    public void DrawsNothing_WhenThereIsLessThanTwoPoints()
    {
        Assert.Equal(string.Empty, RenderLine([], minimumSpan: 1).Find("polyline").GetAttribute("points"));
        Assert.Equal(string.Empty, RenderLine([5.0], minimumSpan: 1).Find("polyline").GetAttribute("points"));
    }

    [Fact]
    public void ThinsOutTooManyValuesAndKeepsTheNewest()
    {
        List<double> values = [.. Enumerable.Range(1, 25).Select(number => (double)number)];

        IRenderedComponent<GwiOSSparkLineUnderTest> line = Render<GwiOSSparkLineUnderTest>(parameters => parameters
            .Add(component => component.Points, [.. values.Select((value, index) => new GwiOSSparkLinePoint(index, value))])
            .Add(component => component.MaxPointCount, 10)
            .Add(component => component.AriaLabel, "Verlauf"));

        string[] points = line.Find("polyline").GetAttribute("points")!.Split(' ');
        Assert.Equal(9, points.Length);
        Assert.Equal("0,22.5", points[0]);
        Assert.Equal("100,1.5", points[^1]);
    }

    [Fact]
    public void ShowsTheAccessibleNameAndTheTooltip()
    {
        IRenderedComponent<GwiOSSparkLineUnderTest> line = Render<GwiOSSparkLineUnderTest>(parameters => parameters
            .Add(component => component.Points, [new GwiOSSparkLinePoint(0, 1), new GwiOSSparkLinePoint(1, 2)])
            .Add(component => component.AriaLabel, "Verlauf des Luftdrucks")
            .Add(component => component.Tooltip, "996,0 bis 997,0 hPa"));

        Assert.Equal("Verlauf des Luftdrucks", line.Find("svg").GetAttribute("aria-label"));
        Assert.Equal("996,0 bis 997,0 hPa", line.Find("svg title").TextContent);
    }

    [Fact]
    public void PlacesThePointsByTheirPosition()
    {
        IRenderedComponent<GwiOSSparkLineUnderTest> line = Render<GwiOSSparkLineUnderTest>(parameters => parameters
            .Add(component => component.Points, [new GwiOSSparkLinePoint(0, 10), new GwiOSSparkLinePoint(60, 11), new GwiOSSparkLinePoint(65, 12)])
            .Add(component => component.MinimumSpan, 1)
            .Add(component => component.AriaLabel, "Verlauf"));

        Assert.Equal("0,22.5 92.31,12 100,1.5", line.Find("polyline").GetAttribute("points"));
    }

    // Values measured a minute apart, so the points are spread evenly over the width.
    private IRenderedComponent<GwiOSSparkLineUnderTest> RenderLine(List<double> values, double minimumSpan)
        => Render<GwiOSSparkLineUnderTest>(parameters => parameters
            .Add(
                component => component.Points,
                [.. values.Select((value, index) => new GwiOSSparkLinePoint(index, value))])
            .Add(component => component.MinimumSpan, minimumSpan)
            .Add(component => component.AriaLabel, "Verlauf"));
}
