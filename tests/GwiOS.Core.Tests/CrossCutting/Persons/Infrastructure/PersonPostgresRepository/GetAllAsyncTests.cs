using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.CrossCutting.Persons.Infrastructure.PersonPostgresRepository;

/// <summary>
/// Covers <c>PersonPostgresRepository.GetAllAsync</c> against the PostgreSQL test database. The database may contain
/// persons of other tests, so the tests only look at the persons they created themselves.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class GetAllAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly IPersonRepository _personRepository = database.PersonRepository;
    private readonly TestPersons _testPersons = new(database.PersonRepository);

    [Fact]
    public async Task ReturnsEveryStoredPersonOnce()
    {
        Person firstPerson = _testPersons.Create();
        Person secondPerson = _testPersons.Create();
        await _personRepository.InsertAsync(firstPerson, TestContext.Current.CancellationToken);
        await _personRepository.InsertAsync(secondPerson, TestContext.Current.CancellationToken);

        List<Person> persons = await _personRepository.GetAllAsync(TestContext.Current.CancellationToken);

        Assert.Single(persons, person => person.Id == firstPerson.Id);
        Assert.Single(persons, person => person.Id == secondPerson.Id);
    }

    [Fact]
    public async Task ReturnsThePersonsOrderedByName()
    {
        string namePrefix = _testPersons.CreateUniqueValue();
        Person personB = _testPersons.Create();
        personB.Name = $"{namePrefix}-b";
        Person personA = _testPersons.Create();
        personA.Name = $"{namePrefix}-a";
        await _personRepository.InsertAsync(personB, TestContext.Current.CancellationToken);
        await _personRepository.InsertAsync(personA, TestContext.Current.CancellationToken);

        List<Person> persons = await _personRepository.GetAllAsync(TestContext.Current.CancellationToken);

        Guid[] expectedIds = [personA.Id, personB.Id];
        Assert.Equal(expectedIds, persons.Where(person => expectedIds.Contains(person.Id)).Select(person => person.Id));
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testPersons.DisposeAsync();
}
