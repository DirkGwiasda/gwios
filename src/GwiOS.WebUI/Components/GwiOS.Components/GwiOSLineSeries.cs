namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// One line of a <see cref="GwiOSLineChart"/>, with one value per label of the chart.
/// </summary>
/// <param name="Label">The name shown in the legend.</param>
/// <param name="Color">The color of the line as a CSS color, e.g. a hex code.</param>
/// <param name="Values">The values of the line, in the same order as the chart's labels. Must not be negative.</param>
public sealed record GwiOSLineSeries(string Label, string Color, IReadOnlyList<decimal> Values);
