namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// One slice of a <see cref="GwiOSDonutChart"/>: a category's share of the total, with the color it is shown in.
/// </summary>
/// <param name="Id">The ID the slice represents, passed back when it is clicked.</param>
/// <param name="Label">The name shown in the legend.</param>
/// <param name="Value">The value of the slice. Must not be negative.</param>
/// <param name="Color">The color of the slice as a CSS color, e.g. a hex code.</param>
public sealed record GwiOSDonutSegment(Guid Id, string Label, decimal Value, string Color);
