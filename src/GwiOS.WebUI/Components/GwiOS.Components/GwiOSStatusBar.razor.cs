using System.Globalization;
using GwiOS.WebUI.StatusMessages.Contracts;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// Status bar at the bottom of the page. It shows the most recent status message of the session and, when clicked,
/// the history of all messages, newest first. It updates itself whenever a message is published.
/// </summary>
public partial class GwiOSStatusBar
{
    private bool _isHistoryOpen;

    [Inject]
    private IStatusMessageService StatusMessages { get; set; } = default!;

    [Inject]
    private TimeProvider TimeProvider { get; set; } = default!;

    private StatusMessage? CurrentMessage
        => StatusMessages.Messages.Count > 0 ? StatusMessages.Messages[^1] : null;

    private IEnumerable<StatusMessage> NewestFirst
        => StatusMessages.Messages.Reverse();

    /// <summary>
    /// Stops updating the status bar when it is removed from the page.
    /// </summary>
    public void Dispose()
        => StatusMessages.MessagesChanged -= OnMessagesChanged;

    protected override void OnInitialized()
        => StatusMessages.MessagesChanged += OnMessagesChanged;

    private void OnMessagesChanged()
        => _ = InvokeAsync(StateHasChanged);

    private void ToggleHistory()
        => _isHistoryOpen = !_isHistoryOpen;

    private string FormatTime(StatusMessage message)
        => TimeZoneInfo.ConvertTime(message.PublishedAt, TimeProvider.LocalTimeZone)
            .ToString("HH:mm", CultureInfo.InvariantCulture);

    private static string GetDotClass(StatusLevel level)
        => level switch
        {
            StatusLevel.Success => "gwios-status-dot--success",
            StatusLevel.Warning => "gwios-status-dot--warning",
            StatusLevel.Error => "gwios-status-dot--error",
            _ => "gwios-status-dot--neutral"
        };
}
