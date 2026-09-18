using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// Small line chart of a series of points, drawn as an SVG that fills the width it is given. Points are placed
/// horizontally by their position, so gaps in time show as wider gaps. Too many points are thinned out evenly, and the
/// scale spans at least <see cref="MinimumSpan"/>, so a quiet series looks flat instead of jagged.
/// </summary>
public partial class GwiOSSparkLine
{
    // The SVG is stretched to the width of its box; these are the coordinates the points are placed in.
    private const double Width = 100;
    private const double Height = 24;

    // Half a line width is kept free at the top and bottom, so the line is not cut off at the edges.
    private const double VerticalPadding = 1.5;

    /// <summary>
    /// The points in order, oldest first. Fewer than two points draw nothing.
    /// </summary>
    [Parameter, EditorRequired]
    public List<GwiOSSparkLinePoint> Points { get; set; } = [];

    /// <summary>
    /// The smallest range the scale covers, widened around the middle of the values.
    /// </summary>
    [Parameter]
    public double MinimumSpan { get; set; } = 1;

    /// <summary>
    /// The largest number of points drawn. More points are thinned out evenly, keeping the newest.
    /// </summary>
    [Parameter]
    public int MaxPointCount { get; set; } = 60;

    /// <summary>
    /// Accessible name of the chart.
    /// </summary>
    [Parameter, EditorRequired]
    public string AriaLabel { get; set; } = string.Empty;

    /// <summary>
    /// Text shown when hovering the chart, or <c>null</c> for none.
    /// </summary>
    [Parameter]
    public string? Tooltip { get; set; }

    private string PointAttribute
        => string.Join(" ", CreatePoints(Thin(Points)));

    private IEnumerable<string> CreatePoints(List<GwiOSSparkLinePoint> points)
    {
        if (points.Count < 2)
        {
            return [];
        }

        (double lowest, double highest) = GetRange(points);
        double firstPosition = points[0].Position;
        double positionSpan = points[^1].Position - firstPosition;
        return points.Select(point => string.Create(
            CultureInfo.InvariantCulture,
            $"{GetX(point.Position - firstPosition, positionSpan):0.##},{GetY(point.Value, lowest, highest):0.##}"));
    }

    // Points are thinned out by taking every nth one, counted from the newest, so the latest point is always drawn.
    private List<GwiOSSparkLinePoint> Thin(List<GwiOSSparkLinePoint> points)
    {
        if (points.Count <= MaxPointCount)
        {
            return points;
        }

        int step = (points.Count / MaxPointCount) + 1;
        return [.. points.Where((_, index) => ((points.Count - 1 - index) % step) == 0)];
    }

    private (double Lowest, double Highest) GetRange(List<GwiOSSparkLinePoint> points)
    {
        double lowest = points.Min(point => point.Value);
        double highest = points.Max(point => point.Value);
        if ((highest - lowest) >= MinimumSpan)
        {
            return (lowest, highest);
        }

        double middle = (lowest + highest) / 2;
        return (middle - (MinimumSpan / 2), middle + (MinimumSpan / 2));
    }

    // Points that all sit at the same position, such as values measured within the same second, are spread evenly.
    private static double GetX(double position, double positionSpan)
        => positionSpan > 0 ? position / positionSpan * Width : Width / 2;

    private static double GetY(double value, double lowest, double highest)
    {
        double usableHeight = Height - (2 * VerticalPadding);
        return Height - VerticalPadding - ((value - lowest) / (highest - lowest) * usableHeight);
    }
}
