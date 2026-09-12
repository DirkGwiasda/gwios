using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.WebUI.StatusMessages.Contracts;
using GwiOS.WebUI.StatusMessages.Contracts.Models;

namespace GwiOS.WebUI.StatusMessages;

internal sealed class StatusMessageService(TimeProvider timeProvider, ILogger<StatusMessageService> logger)
    : IStatusMessageService
{
    internal const int MaxMessageCount = 50;

    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<StatusMessageService> _logger = logger;
    private readonly List<StatusMessage> _messages = [];

    public event Action? MessagesChanged;

    public IReadOnlyList<StatusMessage> Messages
        => _messages;

    public void Publish(StatusLevel level, string text)
    {
        _messages.Add(new StatusMessage(level, text, _timeProvider.GetUtcNow()));
        RemoveMessagesBeyondLimit();
        // The text may contain names, so only the level is logged.
        _logger.LogDebug(
            "Status message published.",
            new Dictionary<string, string> { ["StatusLevel"] = level.ToString() });
        MessagesChanged?.Invoke();
    }

    private void RemoveMessagesBeyondLimit()
    {
        int surplusCount = _messages.Count - MaxMessageCount;
        if (surplusCount > 0)
        {
            _messages.RemoveRange(0, surplusCount);
        }
    }
}
