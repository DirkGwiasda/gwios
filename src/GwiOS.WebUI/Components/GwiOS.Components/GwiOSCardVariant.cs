namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// Layout of a <see cref="GwiOSCard"/>.
/// </summary>
public enum GwiOSCardVariant
{
    /// <summary>
    /// Header and content share the inner spacing of the card, e.g. for charts.
    /// </summary>
    Padded = 0,

    /// <summary>
    /// The header is a bar of its own and the content reaches the edges of the card, e.g. for lists whose rows
    /// are separated by lines.
    /// </summary>
    Flush = 1
}
