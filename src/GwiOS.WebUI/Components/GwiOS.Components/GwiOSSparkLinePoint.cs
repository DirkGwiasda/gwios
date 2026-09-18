namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// A point of a <see cref="GwiOSSparkLine"/>.
/// </summary>
/// <param name="Position">
/// Where the point sits on the horizontal axis, in any unit that grows with time, such as minutes since the first
/// point. Points with wider gaps are drawn wider apart.
/// </param>
/// <param name="Value">The value of the point, which the vertical position is derived from.</param>
public record GwiOSSparkLinePoint(double Position, double Value);
