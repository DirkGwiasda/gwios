using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;

namespace GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;

/// <summary>
/// Manages the persons of GwiOS: creates, changes, reads and deletes them, and rejects persons that are invalid.
/// </summary>
public interface IPersonManager
{
    /// <summary>
    /// Validates and stores the given new person and returns it. Throws a <see cref="PersonValidationException"/>
    /// if the person is invalid and a <see cref="DuplicatePersonException"/> if its ID, name, short name or identity
    /// user ID is already used by another person.
    /// </summary>
    Task<Person> CreatePersonAsync(Person person, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the given person and overwrites the stored person with the same ID. Throws a
    /// <see cref="PersonValidationException"/> if the person is invalid, a <see cref="PersonNotFoundException"/> if
    /// no person with its ID is stored and a <see cref="DuplicatePersonException"/> if its name, short name or identity
    /// user ID is already used by another person.
    /// </summary>
    Task UpdatePersonAsync(Person person, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all stored persons, ordered by name. The list is empty if no persons are stored.
    /// </summary>
    Task<List<Person>> GetAllPersonsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the person linked to the given identity user account, or <c>null</c> if no person is linked to it.
    /// </summary>
    Task<Person?> GetPersonByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the person with the given ID. Does nothing if no such person is stored.
    /// </summary>
    Task DeletePersonAsync(Guid personId, CancellationToken cancellationToken = default);
}
