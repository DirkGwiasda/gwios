using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.Pages.Admin;

/// <summary>
/// Admin page listing the persons of GwiOS, where persons are created and deleted.
/// </summary>
public partial class PersonAdministration
{
    private List<Person> _persons = [];
    private string _newName = string.Empty;
    private string _newShortName = string.Empty;

    [Inject]
    private IPersonManager PersonManager { get; set; } = default!;

    [Inject]
    private IStatusMessageService StatusMessages { get; set; } = default!;

    private bool CanAddPerson
        => !string.IsNullOrWhiteSpace(_newName) && !string.IsNullOrWhiteSpace(_newShortName);

    protected override async Task OnInitializedAsync()
        => await LoadPersonsAsync();

    private async Task AddPersonAsync()
    {
        if (!CanAddPerson)
        {
            return;
        }

        Person person = new() { Name = _newName.Trim(), ShortName = _newShortName.Trim() };
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

    private async Task DeletePersonAsync(Person person)
    {
        await PersonManager.DeletePersonAsync(person.Id);
        StatusMessages.Publish(StatusLevel.Neutral, $"Person „{person.Name}“ gelöscht");
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
