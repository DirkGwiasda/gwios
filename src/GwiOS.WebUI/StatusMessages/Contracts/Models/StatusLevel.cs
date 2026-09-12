namespace GwiOS.WebUI.StatusMessages.Contracts.Models;

/// <summary>
/// How important a status message is; the status bar shows each level in its own color.
/// </summary>
public enum StatusLevel
{
    /// <summary>
    /// A neutral notice, such as a created entry. Shown in gray.
    /// </summary>
    Neutral = 0,

    /// <summary>
    /// A successfully finished action, such as a completed ToDo. Shown in blue.
    /// </summary>
    Success = 1,

    /// <summary>
    /// An action that could not be carried out as requested, such as a name that is already used. Shown in orange.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// A failure the user has to know about. Shown in red.
    /// </summary>
    Error = 3
}
