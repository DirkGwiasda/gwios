using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;

namespace GwiOS.WebUI.Tests.TestInfrastructure;

/// <summary>
/// Stands in for <see cref="IPersonManager"/> by keeping persons in memory, ordered by name. Like the real manager it
/// rejects a person whose name, short name or identity user ID is already used by another person and an update of a
/// person that is not stored, but it does not validate. An update can be held back to observe the page while it saves.
/// </summary>
public sealed class PersonManagerFake : IPersonManager
{
    private readonly List<Person> _persons = [];

    /// <summary>
    /// The stored persons, ordered by name.
    /// </summary>
    public IReadOnlyList<Person> Persons
        => [.. _persons.OrderBy(person => person.Name, StringComparer.Ordinal)];

    /// <summary>
    /// Task every update waits for before it stores the person; already completed unless a test holds updates back.
    /// </summary>
    public Task UpdateReleased { get; set; } = Task.CompletedTask;

    /// <summary>
    /// Stores the given person directly, as if it had been created before the test.
    /// </summary>
    public void Add(Person person)
        => _persons.Add(person);

    /// <summary>
    /// Deletes the given person directly, as if someone else had deleted it.
    /// </summary>
    public void Remove(Person person)
        => _persons.RemoveAll(storedPerson => storedPerson.Id == person.Id);

    public Task<Person> CreatePersonAsync(Person person, CancellationToken cancellationToken = default)
    {
        ThrowIfDuplicate(person);
        _persons.Add(person);
        return Task.FromResult(person);
    }

    public async Task UpdatePersonAsync(Person person, CancellationToken cancellationToken = default)
    {
        await UpdateReleased;
        int index = _persons.FindIndex(storedPerson => storedPerson.Id == person.Id);
        if (index < 0)
        {
            throw new PersonNotFoundException(person.Id);
        }

        ThrowIfDuplicate(person);
        _persons[index] = person;
    }

    public Task<List<Person>> GetAllPersonsAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Persons.ToList());

    public Task<Person?> GetPersonByIdentityUserIdAsync(
        string identityUserId,
        CancellationToken cancellationToken = default)
        => Task.FromResult(_persons.SingleOrDefault(person => person.IdentityUserId == identityUserId));

    public Task DeletePersonAsync(Guid personId, CancellationToken cancellationToken = default)
    {
        _persons.RemoveAll(person => person.Id == personId);
        return Task.CompletedTask;
    }

    private void ThrowIfDuplicate(Person person)
    {
        List<Person> otherPersons = [.. _persons.Where(storedPerson => storedPerson.Id != person.Id)];
        if (otherPersons.Any(otherPerson => otherPerson.Name == person.Name))
        {
            throw CreateDuplicateException(person, nameof(Person.Name));
        }

        if (otherPersons.Any(otherPerson => otherPerson.ShortName == person.ShortName))
        {
            throw CreateDuplicateException(person, nameof(Person.ShortName));
        }

        if ((person.IdentityUserId is not null)
            && otherPersons.Any(otherPerson => otherPerson.IdentityUserId == person.IdentityUserId))
        {
            throw CreateDuplicateException(person, nameof(Person.IdentityUserId));
        }
    }

    private static DuplicatePersonException CreateDuplicateException(Person person, string propertyName)
        => new(person.Id, propertyName, new InvalidOperationException("Unique value already used."));
}
