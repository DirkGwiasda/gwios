using GwiOS.WebUI.StatusMessages.Contracts.Models;

namespace GwiOS.WebUI.StatusMessages.Contracts;

/// <summary>
/// Collects the status messages of the current user session, which the status bar shows. Messages are kept only
/// for the lifetime of the session.
/// </summary>
public interface IStatusMessageService
{
    /// <summary>
    /// Raised after a message was published.
    /// </summary>
    event Action? MessagesChanged;

    /// <summary>
    /// The messages published in this session, oldest first. Holds at most the most recent 50 messages; empty if
    /// none was published yet.
    /// </summary>
    IReadOnlyList<StatusMessage> Messages { get; }

    /// <summary>
    /// Publishes a new message with the given level and text, stamped with the current time, and raises
    /// <see cref="MessagesChanged"/>.
    /// </summary>
    void Publish(StatusLevel level, string text);
}
