using System.Globalization;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Workflows.MeasurementPolling.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteAdoption.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteBlocking.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.Pages;

/// <summary>
/// Page showing a tile with the current values of every satellite and the unknown satellites that pinged recently,
/// which can be adopted or blocked. While automatic refresh is on, the page reads anew every 5 seconds.
/// </summary>
public partial class SatelliteOverview : IDisposable
{
    private static readonly TimeSpan AutoRefreshInterval = TimeSpan.FromSeconds(5);

    private SatelliteOverviewData? _overview;
    private DateTime _now;
    private bool _isAutoRefreshEnabled = true;
    private bool _isPolling;
    private ITimer? _autoRefreshTimer;

    private SatelliteCandidate? _adoptedCandidate;
    private string _adoptDisplayName = string.Empty;
    private string? _displayNameError;
    private bool _isSaving;

    [Inject]
    private ISatelliteOverviewWorkflow SatelliteOverviewWorkflow { get; set; } = default!;

    [Inject]
    private IMeasurementPollingWorkflow MeasurementPollingWorkflow { get; set; } = default!;

    [Inject]
    private ISatelliteAdoptionWorkflow SatelliteAdoptionWorkflow { get; set; } = default!;

    [Inject]
    private ISatelliteBlockingWorkflow SatelliteBlockingWorkflow { get; set; } = default!;

    [Inject]
    private IStatusMessageService StatusMessages { get; set; } = default!;

    [Inject]
    private TimeProvider TimeProvider { get; set; } = default!;

    private List<SatelliteOverviewEntry> Satellites
        => _overview?.Satellites ?? [];

    private List<SatelliteCandidate> Candidates
        => _overview?.Candidates ?? [];

    private int OnlineCount
        => Satellites.Count(entry => entry.IsOnline);

    private int OfflineCount
        => Satellites.Count - OnlineCount;

    private string NetworkText
        => OfflineCount > 0
            ? $"{OnlineCount}/{Satellites.Count} Satelliten aktiv · {OfflineCount} offline"
            : $"{OnlineCount}/{Satellites.Count} Satelliten aktiv";

    private string LastPollText
        => _overview?.LastFetchedAt is { } lastFetchedAt
            ? TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.SpecifyKind(lastFetchedAt, DateTimeKind.Utc)), TimeProvider.LocalTimeZone)
                .ToString("HH:mm:ss", CultureInfo.InvariantCulture)
            : "–";

    protected override async Task OnInitializedAsync()
    {
        await LoadOverviewAsync();
        StartAutoRefresh();
    }

    public void Dispose()
        => StopAutoRefresh();

    private async Task ChangeAutoRefreshAsync(ChangeEventArgs eventArgs)
    {
        _isAutoRefreshEnabled = eventArgs.Value is true;
        if (!_isAutoRefreshEnabled)
        {
            StopAutoRefresh();
            return;
        }

        await LoadOverviewAsync();
        StartAutoRefresh();
    }

    private async Task PollNowAsync()
    {
        if (_isPolling)
        {
            return;
        }

        _isPolling = true;
        StatusMessages.Publish(StatusLevel.Neutral, "Satelliten werden abgefragt");
        try
        {
            await MeasurementPollingWorkflow.PollAllAsync();
            await LoadOverviewAsync();
        }
        finally
        {
            _isPolling = false;
        }
    }

    private void OpenAdoptDialog(SatelliteCandidate candidate)
    {
        _adoptedCandidate = candidate;
        _adoptDisplayName = string.Empty;
        _displayNameError = null;
    }

    private void CloseAdoptDialog()
        => _adoptedCandidate = null;

    private async Task AdoptCandidateAsync()
    {
        if ((_adoptedCandidate is null) || _isSaving)
        {
            return;
        }

        _isSaving = true;
        try
        {
            await AdoptAsync(_adoptedCandidate.Name);
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task AdoptAsync(string candidateName)
    {
        try
        {
            await SatelliteAdoptionWorkflow.AdoptCandidateAsync(candidateName, _adoptDisplayName);
            StatusMessages.Publish(StatusLevel.Success, $"Satellit „{candidateName}“ angelegt");
        }
        catch (SatelliteValidationException)
        {
            _displayNameError = "Der Anzeigename ist ungültig.";
            StatusMessages.Publish(StatusLevel.Error, $"Satellit „{candidateName}“ konnte nicht angelegt werden – bitte Eingaben prüfen");
            return;
        }
        catch (SatelliteCandidateNotFoundException)
        {
            StatusMessages.Publish(StatusLevel.Error, $"Satellit „{candidateName}“ meldet sich nicht mehr");
        }
        catch (SatelliteCandidateTokenChangedException)
        {
            StatusMessages.Publish(StatusLevel.Error, $"Satellit „{candidateName}“ kann nicht angelegt werden – mehrere Absender mit verschiedenen Tokens");
        }
        catch (DuplicateSatelliteException)
        {
            StatusMessages.Publish(StatusLevel.Warning, $"Satellit „{candidateName}“ gibt es bereits");
        }

        CloseAdoptDialog();
        await LoadOverviewAsync();
    }

    private async Task BlockCandidateAsync(SatelliteCandidate candidate)
    {
        try
        {
            await SatelliteBlockingWorkflow.BlockCandidateAsync(candidate.Name);
            StatusMessages.Publish(StatusLevel.Warning, $"Satellit „{candidate.Name}“ blockiert");
        }
        catch (DuplicateSatelliteException)
        {
            StatusMessages.Publish(StatusLevel.Warning, $"Satellit „{candidate.Name}“ gibt es bereits");
        }

        await LoadOverviewAsync();
    }

    private void StartAutoRefresh()
    {
        if (_isAutoRefreshEnabled && (_autoRefreshTimer is null))
        {
            _autoRefreshTimer = TimeProvider.CreateTimer(
                state => _ = InvokeAsync(RefreshAsync),
                state: null,
                AutoRefreshInterval,
                AutoRefreshInterval);
        }
    }

    private void StopAutoRefresh()
    {
        _autoRefreshTimer?.Dispose();
        _autoRefreshTimer = null;
    }

    private async Task RefreshAsync()
    {
        if (_autoRefreshTimer is null)
        {
            return;
        }

        await LoadOverviewAsync();
        StateHasChanged();
    }

    private async Task LoadOverviewAsync()
    {
        _overview = await SatelliteOverviewWorkflow.GetOverviewAsync();
        _now = TimeProvider.GetUtcNow().UtcDateTime;
    }
}
