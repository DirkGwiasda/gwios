using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;

namespace GwiOS.Core.Tests.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;

/// <summary>
/// Stands in for <see cref="IPersonRepository"/> by keeping persons in memory. It orders, finds, updates and deletes
/// like the real repository, but does not enforce unique values.
/// </summary>
public sealed class PersonRepositoryFake : IPersonRepository
{
    private readonly List<Person> _persons = [];

    public Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task InsertAsync(Person person, CancellationToken cancellationToken = default)
    {
        _persons.Add(person);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Person person, CancellationToken cancellationToken = default)
    {
        int index = _persons.FindIndex(storedPerson => storedPerson.Id == person.Id);
        if (index < 0)
        {
            throw new PersonNotFoundException(person.Id);
        }

        _persons[index] = person;
        return Task.CompletedTask;
    }

    public Task<List<Person>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_persons.OrderBy(person => person.Name, StringComparer.Ordinal).ToList());

    public Task<Person?> GetByIdentityUserIdAsync(string identityUserId, CancellationToken cancellationToken = default)
        => Task.FromResult(_persons.SingleOrDefault(person => person.IdentityUserId == identityUserId));

    public Task DeleteAsync(Guid personId, CancellationToken cancellationToken = default)
    {
        _persons.RemoveAll(person => person.Id == personId);
        return Task.CompletedTask;
    }
}
