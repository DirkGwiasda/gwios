using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;

namespace GwiOS.WebUI.Tests.TestInfrastructure;

/// <summary>
/// Stands in for <see cref="IPersonManager"/> by keeping persons in memory, ordered by name. Like the real manager it
/// rejects a new person whose name or short name is already used, but it does not validate.
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
    /// Stores the given person directly, as if it had been created before the test.
    /// </summary>
    public void Add(Person person)
        => _persons.Add(person);

    public Task<Person> CreatePersonAsync(Person person, CancellationToken cancellationToken = default)
    {
        ThrowIfDuplicate(person);
        _persons.Add(person);
        return Task.FromResult(person);
    }

    public Task UpdatePersonAsync(Person person, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("The WebUI does not update persons yet.");

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
        if (_persons.Any(storedPerson => storedPerson.Name == person.Name))
        {
            throw CreateDuplicateException(person, nameof(Person.Name));
        }

        if (_persons.Any(storedPerson => storedPerson.ShortName == person.ShortName))
        {
            throw CreateDuplicateException(person, nameof(Person.ShortName));
        }
    }

    private static DuplicatePersonException CreateDuplicateException(Person person, string propertyName)
        => new(person.Id, propertyName, new InvalidOperationException("Unique value already used."));
}
