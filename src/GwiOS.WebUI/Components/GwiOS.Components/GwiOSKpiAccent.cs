namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// Color of a part of a <see cref="GwiOSKpiTile"/>: its left border, its value or the right text of its footer.
/// </summary>
public enum GwiOSKpiAccent
{
    /// <summary>
    /// The primary blue, for neutral figures.
    /// </summary>
    Primary = 0,

    /// <summary>
    /// Green, for positive figures such as income.
    /// </summary>
    Success = 1,

    /// <summary>
    /// Red, for negative figures such as expenses.
    /// </summary>
    Danger = 2,

    /// <summary>
    /// Yellow, for figures that need attention such as open amounts.
    /// </summary>
    Warning = 3,

    /// <summary>
    /// A faint gray, for figures without significance such as "nichts offen".
    /// </summary>
    Muted = 4
}
