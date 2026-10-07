using GwiOS.WebUI.Components.GwiOS.Components;
using GwiOSLineChartLegendUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSLineChartLegend;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSLineChartLegend;

/// <summary>
/// Covers how <c>GwiOSLineChartLegend</c> lists the series of a line chart.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Fact]
    public void RendersAnEntryWithTheColorOfEachSeriesInOrder()
    {
        IRenderedComponent<GwiOSLineChartLegendUnderTest> legend = Render<GwiOSLineChartLegendUnderTest>(parameters => parameters
            .Add(component => component.Series, [
                new GwiOSLineSeries("Einnahmen", "#00ffa3", [1m]),
                new GwiOSLineSeries("Ausgaben", "#ff4d6d", [2m])
            ]));

        Assert.Equal(["Einnahmen", "Ausgaben"], legend.FindAll(".gwios-line-chart-legend-item").Select(item => item.TextContent.Trim()));
        Assert.Contains("#ff4d6d", legend.FindAll(".gwios-line-chart-legend-swatch")[1].GetAttribute("style"));
    }
}
