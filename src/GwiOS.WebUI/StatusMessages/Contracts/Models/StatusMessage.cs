namespace GwiOS.WebUI.StatusMessages.Contracts.Models;

/// <summary>
/// A message shown to the user in the status bar.
/// </summary>
/// <param name="Level">How important the message is.</param>
/// <param name="Text">The text shown to the user, in German.</param>
/// <param name="PublishedAt">Point in time the message was published.</param>
public record StatusMessage(StatusLevel Level, string Text, DateTimeOffset PublishedAt)
{
    /// <summary>
    /// Unique identifier of the message. Defaults to a new time-ordered UUID (version 7).
    /// </summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();
}
