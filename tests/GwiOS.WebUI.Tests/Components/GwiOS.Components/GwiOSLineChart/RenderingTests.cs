using System.Globalization;
using GwiOS.WebUI.Components.GwiOS.Components;
using GwiOSLineChartUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSLineChart;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSLineChart;

/// <summary>
/// Covers how <c>GwiOSLineChart</c> draws its series and labels.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Fact]
    public void DrawsALineOfTheSeriesColorPerSeries()
    {
        IRenderedComponent<GwiOSLineChartUnderTest> chart = RenderChart();

        Assert.Equal(["#00ffa3", "#ff4d6d"], chart.FindAll("polyline").Select(line => line.GetAttribute("stroke")));
    }

    [Fact]
    public void SpreadsThePointsOverTheWidthAndScalesThemToTheLargestValue()
    {
        IRenderedComponent<GwiOSLineChartUnderTest> chart = RenderChart();

        Assert.Equal("0,10 360,105 720,200", chart.FindAll("polyline")[0].GetAttribute("points"));
    }

    [Fact]
    public void ShowsTheLargestValueRoundedToWholeEuros()
    {
        IRenderedComponent<GwiOSLineChartUnderTest> chart = RenderChart();

        Assert.Equal(5450m.ToString("C", CultureInfo.GetCultureInfo("de-DE")), chart.Find(".gwios-line-chart-max-label").TextContent);
    }

    [Fact]
    public void ShowsTheLabelsInOrder()
    {
        IRenderedComponent<GwiOSLineChartUnderTest> chart = RenderChart();

        Assert.Equal(["Aug", "Sep", "Okt"], chart.FindAll(".gwios-line-chart-label").Select(label => label.TextContent));
    }

    [Fact]
    public void RendersNoLegendOfItsOwn()
    {
        IRenderedComponent<GwiOSLineChartUnderTest> chart = RenderChart();

        Assert.Empty(chart.FindAll(".gwios-line-chart-legend"));
    }

    private IRenderedComponent<GwiOSLineChartUnderTest> RenderChart()
        => Render<GwiOSLineChartUnderTest>(parameters => parameters
            .Add(component => component.Series, [
                new GwiOSLineSeries("Einnahmen", "#00ffa3", [5449.6m, 2724.8m, 0m]),
                new GwiOSLineSeries("Ausgaben", "#ff4d6d", [100m, 200m, 300m])
            ])
            .Add(component => component.Labels, ["Aug", "Sep", "Okt"]));
}
