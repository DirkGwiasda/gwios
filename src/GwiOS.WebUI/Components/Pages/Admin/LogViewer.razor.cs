using System.Globalization;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.Pages.Admin;

/// <summary>
/// Admin page showing the stored log entries of one application at a time, newest first, where the entries of an
/// application can also be deleted.
/// </summary>
public partial class LogViewer
{
    private List<string> _appNames = [];
    private string? _selectedAppName;
    private List<LogEntry> _entries = [];

    [Inject]
    private ILogEntryManager LogEntryManager { get; set; } = default!;

    [Inject]
    private IStatusMessageService StatusMessages { get; set; } = default!;

    [Inject]
    private TimeProvider TimeProvider { get; set; } = default!;

    protected override async Task OnInitializedAsync()
        => await LoadAppNamesAsync();

    private async Task SelectAppAsync(string appName)
    {
        _selectedAppName = appName;
        await ReloadEntriesAsync();
    }

    private async Task ReloadEntriesAsync()
        => _entries = _selectedAppName is null
            ? []
            : await LogEntryManager.GetAllLogEntriesByAppAsync(_selectedAppName);

    private async Task DeleteEntriesAsync()
    {
        if (_selectedAppName is null)
        {
            return;
        }

        string appName = _selectedAppName;
        await LogEntryManager.DeleteByAppNameAsync(appName);
        StatusMessages.Publish(StatusLevel.Neutral, $"Log-Einträge von „{appName}“ gelöscht");
        await LoadAppNamesAsync();
    }

    private async Task LoadAppNamesAsync()
    {
        _appNames = await LogEntryManager.GetAllAppNamesAsync();
        _selectedAppName = _appNames.FirstOrDefault();
        await ReloadEntriesAsync();
    }

    private string FormatTimestamp(LogEntry entry)
        => TimeZoneInfo.ConvertTime(entry.Timestamp, TimeProvider.LocalTimeZone)
            .ToString("dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
}
