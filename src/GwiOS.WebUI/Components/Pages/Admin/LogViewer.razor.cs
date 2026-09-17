using GwiOS.Core.CrossCutting.Logging.Domain.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace GwiOS.WebUI.Components.Pages.Admin;

/// <summary>
/// Admin page showing the stored log entries page by page, newest first, either of all applications or of one
/// application, whose entries can also be deleted. A click on an entry opens its details in a dialog.
/// </summary>
public partial class LogViewer
{
    // Label of the choice that shows the entries of all applications.
    private const string AllAppsChoice = "Alle";

    // The table shows at most this many characters of the first line of a message.
    private const int MaxShortMessageLength = 90;

    private List<string> _appNames = [];
    private string? _selectedAppName;
    private int _pageNumber = 1;
    private int _pageSize = 10;
    private LogEntryPage _page = new([], 0);
    private LogEntry? _openedEntry;

    [Inject]
    private ILogEntryManager LogEntryManager { get; set; } = default!;

    [Inject]
    private IStatusMessageService StatusMessages { get; set; } = default!;

    private List<string> AppChoices
        => [AllAppsChoice, .. _appNames];

    private string SelectedAppChoice
        => _selectedAppName ?? AllAppsChoice;

    private int LastPageNumber
        => Math.Max(1, (int)Math.Ceiling(_page.TotalCount / (double)_pageSize));

    protected override async Task OnInitializedAsync()
        => await LoadAppNamesAsync();

    private async Task SelectAppAsync(string appChoice)
    {
        _selectedAppName = appChoice == AllAppsChoice ? null : appChoice;
        _pageNumber = 1;
        await ReloadEntriesAsync();
    }

    private async Task ChangePageAsync(int pageNumber)
    {
        _pageNumber = pageNumber;
        await ReloadEntriesAsync();
    }

    private async Task ChangePageSizeAsync(int pageSize)
    {
        _pageSize = pageSize;
        _pageNumber = 1;
        await ReloadEntriesAsync();
    }

    private async Task ReloadEntriesAsync()
    {
        _page = await LoadPageAsync();
        if (IsBeyondLastPage())
        {
            _pageNumber = LastPageNumber;
            _page = await LoadPageAsync();
        }
    }

    // Entries deleted since the page was opened can leave the current page number beyond the last page.
    private bool IsBeyondLastPage()
        => (_page.Entries.Count == 0) && (_pageNumber > LastPageNumber);

    private async Task<LogEntryPage> LoadPageAsync()
        => _selectedAppName is null
            ? await LogEntryManager.GetLogEntryPageAsync(_pageNumber, _pageSize)
            : await LogEntryManager.GetLogEntryPageByAppAsync(_selectedAppName, _pageNumber, _pageSize);

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
        if ((_selectedAppName is not null) && !_appNames.Contains(_selectedAppName))
        {
            _selectedAppName = null;
        }

        _pageNumber = 1;
        await ReloadEntriesAsync();
    }

    private void OpenEntry(LogEntry entry)
        => _openedEntry = entry;

    private void OpenEntryOnEnter(KeyboardEventArgs eventArgs, LogEntry entry)
    {
        if (eventArgs.Key == "Enter")
        {
            OpenEntry(entry);
        }
    }

    private void CloseEntry()
        => _openedEntry = null;

    private static string ShortenMessage(string message)
    {
        string firstLine = message.Split('\n')[0].TrimEnd('\r');
        return firstLine.Length > MaxShortMessageLength
            ? $"{firstLine[..MaxShortMessageLength]} …"
            : firstLine;
    }
}
