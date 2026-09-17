using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.Pages.Admin;

/// <summary>
/// Admin page listing the persons of GwiOS, where persons are created, changed in a dialog and deleted.
/// </summary>
public partial class PersonAdministration
{
    private const string NameRequiredMessage = "Der Name ist ein Pflichtfeld.";
    private const string ShortNameRequiredMessage = "Der Kurzname ist ein Pflichtfeld.";

    private List<Person> _persons = [];
    private string _newName = string.Empty;
    private string _newShortName = string.Empty;

    private Person? _editedPerson;
    private string _editName = string.Empty;
    private string _editShortName = string.Empty;
    private string _editIdentityUserId = string.Empty;
    private string? _nameError;
    private string? _shortNameError;
    private string? _identityUserIdError;
    private bool _isSaving;

    [Inject]
    private IPersonManager PersonManager { get; set; } = default!;

    [Inject]
    private IStatusMessageService StatusMessages { get; set; } = default!;

    private bool CanAddPerson
        => !string.IsNullOrWhiteSpace(_newName);

    protected override async Task OnInitializedAsync()
        => await LoadPersonsAsync();

    private async Task AddPersonAsync()
    {
        if (!CanAddPerson)
        {
            return;
        }

        Person person = new() { Name = _newName.Trim(), ShortName = GetNewShortName() };
        try
        {
            await PersonManager.CreatePersonAsync(person);
        }
        catch (DuplicatePersonException exception)
        {
            StatusMessages.Publish(StatusLevel.Warning, DescribeDuplicate(person, exception.PropertyName));
            return;
        }

        _newName = string.Empty;
        _newShortName = string.Empty;
        StatusMessages.Publish(StatusLevel.Neutral, $"Person „{person.Name}“ hinzugefügt");
        await LoadPersonsAsync();
    }

    // Without a short name, the first word of the name is used, which usually is the first name.
    private string GetNewShortName()
        => string.IsNullOrWhiteSpace(_newShortName)
            ? _newName.Trim().Split(' ')[0]
            : _newShortName.Trim();

    private async Task DeletePersonAsync(Person person)
    {
        await PersonManager.DeletePersonAsync(person.Id);
        StatusMessages.Publish(StatusLevel.Warning, $"Person „{person.Name}“ gelöscht");
        await LoadPersonsAsync();
    }

    private void OpenEditor(Person person)
    {
        _editedPerson = person;
        _editName = person.Name;
        _editShortName = person.ShortName;
        _editIdentityUserId = person.IdentityUserId ?? string.Empty;
        _nameError = null;
        _shortNameError = null;
        _identityUserIdError = null;
    }

    private void CloseEditor()
        => _editedPerson = null;

    private void ClearNameError()
        => _nameError = null;

    private void ClearShortNameError()
        => _shortNameError = null;

    private void ClearIdentityUserIdError()
        => _identityUserIdError = null;

    private async Task SavePersonAsync()
    {
        if ((_editedPerson is null) || _isSaving)
        {
            return;
        }

        if (!ValidateEditor())
        {
            PublishSaveFailure();
            return;
        }

        _isSaving = true;
        try
        {
            await StoreChangesAsync(_editedPerson);
        }
        finally
        {
            _isSaving = false;
        }
    }

    // Checks the required fields and shows an error at each empty one; returns whether all are filled in.
    private bool ValidateEditor()
    {
        _nameError = string.IsNullOrWhiteSpace(_editName) ? NameRequiredMessage : null;
        _shortNameError = string.IsNullOrWhiteSpace(_editShortName) ? ShortNameRequiredMessage : null;
        return (_nameError is null) && (_shortNameError is null);
    }

    private async Task StoreChangesAsync(Person storedPerson)
    {
        Person changedPerson = CreateChangedPerson(storedPerson);
        try
        {
            await PersonManager.UpdatePersonAsync(changedPerson);
        }
        catch (DuplicatePersonException exception)
        {
            ShowDuplicateError(exception.PropertyName);
            PublishSaveFailure();
            return;
        }
        catch (PersonNotFoundException)
        {
            StatusMessages.Publish(StatusLevel.Error, $"Person „{storedPerson.Name}“ gibt es nicht mehr");
            await CloseEditorAndReloadPersonsAsync();
            return;
        }

        StatusMessages.Publish(StatusLevel.Success, $"Person „{changedPerson.Name}“ gespeichert");
        await CloseEditorAndReloadPersonsAsync();
    }

    private Person CreateChangedPerson(Person storedPerson)
        => new()
        {
            Id = storedPerson.Id,
            Name = _editName.Trim(),
            ShortName = _editShortName.Trim(),
            IdentityUserId = string.IsNullOrWhiteSpace(_editIdentityUserId) ? null : _editIdentityUserId.Trim()
        };

    private void ShowDuplicateError(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(Person.Name):
                _nameError = "Dieser Name ist bereits vergeben";
                break;
            case nameof(Person.ShortName):
                _shortNameError = "Dieser Kurzname ist bereits vergeben";
                break;
            case nameof(Person.IdentityUserId):
                _identityUserIdError = "Dieses Benutzerkonto ist bereits verknüpft";
                break;
        }
    }

    private void PublishSaveFailure()
        => StatusMessages.Publish(StatusLevel.Error, "Person konnte nicht gespeichert werden – bitte Eingaben prüfen");

    private async Task CloseEditorAndReloadPersonsAsync()
    {
        CloseEditor();
        await LoadPersonsAsync();
    }

    private async Task LoadPersonsAsync()
        => _persons = await PersonManager.GetAllPersonsAsync();

    private static string DescribeDuplicate(Person person, string propertyName)
        => propertyName switch
        {
            nameof(Person.Name) => $"Der Name „{person.Name}“ ist bereits vergeben",
            nameof(Person.ShortName) => $"Der Kurzname „{person.ShortName}“ ist bereits vergeben",
            _ => $"Person „{person.Name}“ konnte nicht angelegt werden, weil ein Wert bereits vergeben ist"
        };
}
