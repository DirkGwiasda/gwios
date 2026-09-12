using GwiOS.WebUI.StatusMessages.Contracts;
using GwiOS.WebUI.StatusMessages.Contracts.Models;

namespace GwiOS.WebUI.Tests.TestInfrastructure;

/// <summary>
/// Stands in for <see cref="IStatusMessageService"/> by keeping every published message in memory, stamped with a
/// fixed time, and raising <see cref="MessagesChanged"/> like the real service.
/// </summary>
public sealed class StatusMessageServiceFake : IStatusMessageService
{
    private readonly List<StatusMessage> _messages = [];

    public event Action? MessagesChanged;

    /// <summary>
    /// The point in time every published message is stamped with. Defaults to 2026-07-14 10:30 UTC.
    /// </summary>
    public DateTimeOffset PublishTime { get; set; } = new(2026, 7, 14, 10, 30, 0, TimeSpan.Zero);

    public IReadOnlyList<StatusMessage> Messages
        => _messages;

    /// <summary>
    /// The number of handlers currently subscribed to <see cref="MessagesChanged"/>.
    /// </summary>
    public int SubscriberCount
        => MessagesChanged?.GetInvocationList().Length ?? 0;

    public void Publish(StatusLevel level, string text)
    {
        _messages.Add(new StatusMessage(level, text, PublishTime));
        MessagesChanged?.Invoke();
    }
}
