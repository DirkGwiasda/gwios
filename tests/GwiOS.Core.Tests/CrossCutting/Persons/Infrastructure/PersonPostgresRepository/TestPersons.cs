using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;

namespace GwiOS.Core.Tests.CrossCutting.Persons.Infrastructure.PersonPostgresRepository;

/// <summary>
/// Creates persons with unique values for the tests against the PostgreSQL test database and deletes every person
/// it created from the database when it is disposed. The database may contain persons of other tests, so tests
/// only look at the persons they created themselves.
/// </summary>
public sealed class TestPersons(IPersonRepository personRepository) : IAsyncDisposable
{
    private readonly IPersonRepository _personRepository = personRepository;
    private readonly List<Guid> _personIds = [];

    /// <summary>
    /// Returns a new, not yet stored person whose name, short name and identity user ID are all unique.
    /// </summary>
    public Person Create()
    {
        Person person = new()
        {
            Name = CreateUniqueValue(),
            ShortName = CreateUniqueValue(),
            IdentityUserId = CreateUniqueValue()
        };
        _personIds.Add(person.Id);
        return person;
    }

    /// <summary>
    /// Returns a value that no other person uses, prefixed so that leftovers are recognizable as test data.
    /// </summary>
    public string CreateUniqueValue()
        => $"test-{Guid.NewGuid():N}";

    /// <summary>
    /// Returns the stored person with the given ID, or <c>null</c> if no such person is stored.
    /// </summary>
    public async Task<Person?> FindStoredAsync(Guid personId)
        => (await _personRepository.GetAllAsync()).SingleOrDefault(person => person.Id == personId);

    public async ValueTask DisposeAsync()
    {
        foreach (Guid personId in _personIds)
        {
            await _personRepository.DeleteAsync(personId);
        }
    }
}
