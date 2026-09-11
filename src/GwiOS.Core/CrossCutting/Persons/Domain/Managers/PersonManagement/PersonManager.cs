using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;

namespace GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement;

internal sealed class PersonManager(
    IPersonRepository personRepository,
    IPersonValidator personValidator,
    ILogger<PersonManager> logger) : IPersonManager
{
    private readonly IPersonRepository _personRepository = personRepository;
    private readonly IPersonValidator _personValidator = personValidator;
    private readonly ILogger<PersonManager> _logger = logger;

    public async Task<Person> CreatePersonAsync(Person person, CancellationToken cancellationToken = default)
    {
        ThrowIfInvalid(person);
        await _personRepository.InsertAsync(person, cancellationToken);
        _logger.LogInformation("Person created.", CreatePersonIdData(person.Id));
        return person;
    }

    public async Task UpdatePersonAsync(Person person, CancellationToken cancellationToken = default)
    {
        ThrowIfInvalid(person);
        await _personRepository.UpdateAsync(person, cancellationToken);
        _logger.LogInformation("Person updated.", CreatePersonIdData(person.Id));
    }

    public async Task<List<Person>> GetAllPersonsAsync(CancellationToken cancellationToken = default)
        => await _personRepository.GetAllAsync(cancellationToken);

    public async Task<Person?> GetPersonByIdentityUserIdAsync(
        string identityUserId,
        CancellationToken cancellationToken = default)
        => await _personRepository.GetByIdentityUserIdAsync(identityUserId, cancellationToken);

    public async Task DeletePersonAsync(Guid personId, CancellationToken cancellationToken = default)
    {
        await _personRepository.DeleteAsync(personId, cancellationToken);
        _logger.LogInformation("Person deleted.", CreatePersonIdData(personId));
    }

    private void ThrowIfInvalid(Person person)
    {
        if (!_personValidator.IsValid(person, out List<string> validationErrors))
        {
            throw new PersonValidationException(person.Id, validationErrors);
        }
    }

    private static Dictionary<string, string> CreatePersonIdData(Guid personId)
        => new() { ["PersonId"] = personId.ToString() };
}
