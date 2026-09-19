using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// Donut chart of a category breakdown, drawn with a conic gradient, with a legend that can be clicked to drill
/// into a segment.
/// </summary>
public partial class GwiOSDonutChart
{
    private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>
    /// The slices of the chart, largest first.
    /// </summary>
    [Parameter, EditorRequired]
    public IReadOnlyList<GwiOSDonutSegment> Segments { get; set; } = [];

    /// <summary>
    /// Raised with the ID of the segment the user clicks, to drill into it.
    /// </summary>
    [Parameter]
    public EventCallback<Guid> OnSegmentClick { get; set; }

    private decimal Total
        => Segments.Sum(segment => segment.Value);

    private string GradientStyle
    {
        get
        {
            if (Segments.Count == 0)
            {
                return "conic-gradient(rgba(124, 147, 179, .25) 0deg 360deg)";
            }

            decimal total = Total == 0 ? 1 : Total;
            decimal accumulated = 0;
            List<string> stops = [];
            foreach (GwiOSDonutSegment segment in Segments)
            {
                decimal from = (accumulated / total) * 360;
                accumulated += segment.Value;
                decimal to = (accumulated / total) * 360;
                stops.Add($"{segment.Color} {from.ToString("0.##", CultureInfo.InvariantCulture)}deg {to.ToString("0.##", CultureInfo.InvariantCulture)}deg");
            }

            return $"conic-gradient({string.Join(", ", stops)})";
        }
    }

    private string FormatValue(decimal value)
        => value.ToString("C", GermanCulture);

    private string FormatPercentage(decimal value)
    {
        decimal total = Total;
        int percentage = total == 0 ? 0 : (int)Math.Round((value / total) * 100, MidpointRounding.AwayFromZero);
        return $"{percentage} %";
    }
}
