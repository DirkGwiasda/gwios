using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;

namespace GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;

/// <summary>
/// Persists, reads and deletes persons in the underlying storage and keeps their ID, name, short name and identity
/// user ID unique.
/// </summary>
public interface IPersonRepository
{
    /// <summary>
    /// Creates the tables required for persons unless they already exist. The database itself must already exist.
    /// The operation is idempotent: calling it against existing tables changes nothing.
    /// </summary>
    Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores the given new person. Throws a <see cref="DuplicatePersonException"/> if its ID, name, short name or
    /// identity user ID is already used by a stored person.
    /// </summary>
    Task InsertAsync(Person person, CancellationToken cancellationToken = default);

    /// <summary>
    /// Overwrites the stored person with the ID of the given person. Throws a <see cref="PersonNotFoundException"/>
    /// if no such person is stored and a <see cref="DuplicatePersonException"/> if its name, short name or identity
    /// user ID is already used by another stored person.
    /// </summary>
    Task UpdateAsync(Person person, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all stored persons, ordered by name. The list is empty if no persons are stored.
    /// </summary>
    Task<List<Person>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the person linked to the given identity user account, or <c>null</c> if no person is linked to it.
    /// </summary>
    Task<Person?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the person with the given ID. Does nothing if no such person is stored.
    /// </summary>
    Task DeleteAsync(Guid personId, CancellationToken cancellationToken = default);
}
