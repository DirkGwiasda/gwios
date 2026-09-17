using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.Pages.Admin;

/// <summary>
/// Admin page listing the satellites of GwiOS, where satellites are created, renamed, locked, unlocked, given a new
/// token and deleted. A new token is shown once in a dialog.
/// </summary>
public partial class SatelliteAdministration
{
    private const string NameRequiredMessage = "Der Name ist ein Pflichtfeld.";

    private List<SatelliteOverviewEntry> _entries = [];

    private bool _isCreateDialogOpen;
    private string _newName = string.Empty;
    private string _newDisplayName = string.Empty;
    private string? _nameError;

    private Satellite? _editedSatellite;
    private string _editDisplayName = string.Empty;

    private ShownToken? _shownToken;
    private bool _isSaving;

    [Inject]
    private ISatelliteManager SatelliteManager { get; set; } = default!;

    [Inject]
    private ISatelliteOverviewWorkflow SatelliteOverviewWorkflow { get; set; } = default!;

    [Inject]
    private IStatusMessageService StatusMessages { get; set; } = default!;

    protected override async Task OnInitializedAsync()
        => await LoadSatellitesAsync();

    private void OpenCreateDialog()
    {
        _isCreateDialogOpen = true;
        _newName = string.Empty;
        _newDisplayName = string.Empty;
        _nameError = null;
    }

    private void CloseCreateDialog()
        => _isCreateDialogOpen = false;

    private void ClearNameError()
        => _nameError = null;

    private async Task CreateSatelliteAsync()
    {
        if (!_isCreateDialogOpen || _isSaving)
        {
            return;
        }

        // Names consist of lowercase letters only, so an uppercase letter typed by mistake is corrected.
        string name = _newName.Trim().ToLowerInvariant();
        if (name.Length == 0)
        {
            _nameError = NameRequiredMessage;
            return;
        }

        _isSaving = true;
        try
        {
            await StoreNewSatelliteAsync(name);
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task StoreNewSatelliteAsync(string name)
    {
        string token;
        try
        {
            token = await SatelliteManager.CreateSatelliteAsync(name, _newDisplayName);
        }
        catch (SatelliteValidationException)
        {
            _nameError = "Nur a–z, 0–9 und Bindestrich, 1–32 Zeichen.";
            StatusMessages.Publish(StatusLevel.Error, "Satellit konnte nicht angelegt werden – Name ungültig");
            return;
        }
        catch (DuplicateSatelliteException)
        {
            _nameError = "Dieser Name ist bereits vergeben.";
            StatusMessages.Publish(StatusLevel.Error, "Satellit konnte nicht angelegt werden – Name doppelt");
            return;
        }

        CloseCreateDialog();
        StatusMessages.Publish(StatusLevel.Success, $"Satellit „{name}“ angelegt");
        _shownToken = new ShownToken(name, token);
        await LoadSatellitesAsync();
    }

    private void OpenEditDialog(Satellite satellite)
    {
        _editedSatellite = satellite;
        _editDisplayName = satellite.DisplayName ?? string.Empty;
    }

    private void CloseEditDialog()
        => _editedSatellite = null;

    private async Task SaveSatelliteAsync()
    {
        if ((_editedSatellite is null) || _isSaving)
        {
            return;
        }

        _isSaving = true;
        try
        {
            await StoreDisplayNameAsync(_editedSatellite.Name);
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task StoreDisplayNameAsync(string name)
    {
        try
        {
            await SatelliteManager.ChangeDisplayNameAsync(name, _editDisplayName);
            StatusMessages.Publish(StatusLevel.Success, $"Satellit „{name}“ gespeichert");
        }
        catch (SatelliteValidationException)
        {
            StatusMessages.Publish(StatusLevel.Error, $"Satellit „{name}“ konnte nicht gespeichert werden – Anzeigename ungültig");
            return;
        }
        catch (SatelliteNotFoundException)
        {
            PublishNotFound(name);
        }

        CloseEditDialog();
        await LoadSatellitesAsync();
    }

    private async Task ToggleLockAsync(Satellite satellite)
    {
        try
        {
            await ChangeLockAsync(satellite);
        }
        catch (SatelliteNotFoundException)
        {
            PublishNotFound(satellite.Name);
        }

        await LoadSatellitesAsync();
    }

    private async Task ChangeLockAsync(Satellite satellite)
    {
        if (satellite.IsLocked)
        {
            await SatelliteManager.UnlockSatelliteAsync(satellite.Name);
            StatusMessages.Publish(StatusLevel.Success, $"Satellit „{satellite.Name}“ entsperrt");
            return;
        }

        await SatelliteManager.LockSatelliteAsync(satellite.Name);
        StatusMessages.Publish(StatusLevel.Warning, $"Satellit „{satellite.Name}“ gesperrt");
    }

    private async Task RenewTokenAsync(Satellite satellite)
    {
        try
        {
            string token = await SatelliteManager.RenewTokenAsync(satellite.Name);
            StatusMessages.Publish(StatusLevel.Warning, $"Neues Token für „{satellite.Name}“ erzeugt");
            _shownToken = new ShownToken(satellite.Name, token);
        }
        catch (SatelliteNotFoundException)
        {
            PublishNotFound(satellite.Name);
        }

        await LoadSatellitesAsync();
    }

    private async Task DeleteSatelliteAsync(Satellite satellite)
    {
        await SatelliteManager.DeleteSatelliteAsync(satellite.Name);
        StatusMessages.Publish(StatusLevel.Warning, $"Satellit „{satellite.Name}“ gelöscht");
        await LoadSatellitesAsync();
    }

    // The token is dropped as soon as the dialog closes, so it cannot be shown a second time.
    private void CloseTokenDialog()
        => _shownToken = null;

    private void PublishNotFound(string name)
        => StatusMessages.Publish(StatusLevel.Error, $"Satellit „{name}“ gibt es nicht mehr");

    private async Task LoadSatellitesAsync()
        => _entries = (await SatelliteOverviewWorkflow.GetOverviewAsync()).Satellites;

    private sealed record ShownToken(string SatelliteName, string Token);
}
