using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// Line chart comparing one or more series (e.g. income against expenses) over the given labels (e.g. months). Its
/// legend is a separate <see cref="GwiOSLineChartLegend"/>, so it can be placed e.g. in the header of a card.
/// </summary>
public partial class GwiOSLineChart
{
    private const double ChartWidth = 720;
    private const double ChartHeight = 200;

    private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>
    /// The lines of the chart. Each must have as many values as <see cref="Labels"/> has entries.
    /// </summary>
    [Parameter, EditorRequired]
    public IReadOnlyList<GwiOSLineSeries> Series { get; set; } = [];

    /// <summary>
    /// The labels along the x-axis, e.g. abbreviated month names.
    /// </summary>
    [Parameter, EditorRequired]
    public IReadOnlyList<string> Labels { get; set; } = [];

    private decimal MaxValue
        => Series.SelectMany(series => series.Values).DefaultIfEmpty(0).Max();

    private string MaxLabel
        => Math.Round(MaxValue).ToString("C", GermanCulture);

    private string GetPoints(GwiOSLineSeries series)
    {
        decimal max = MaxValue == 0 ? 1 : MaxValue;
        int count = series.Values.Count;
        List<string> points = [];
        for (int index = 0; index < count; index++)
        {
            double x = count > 1 ? (index / (double)(count - 1)) * ChartWidth : ChartWidth / 2;
            double y = ChartHeight - ((double)(series.Values[index] / max) * (ChartHeight - 10));
            points.Add($"{Math.Round(x)},{Math.Round(y)}");
        }

        return string.Join(' ', points);
    }
}
