using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.CrossCutting.Persons.Infrastructure.PersonPostgresRepository;

/// <summary>
/// Covers <c>PersonPostgresRepository.UpdateAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class UpdateAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly PostgresTestDatabase _database = database;
    private readonly IPersonRepository _personRepository = database.PersonRepository;
    private readonly TestPersons _testPersons = new(database.PersonRepository);

    [Fact]
    public async Task OverwritesAllPropertiesOfTheStoredPerson()
    {
        Person person = _testPersons.Create();
        await _personRepository.InsertAsync(person, TestContext.Current.CancellationToken);
        Person changedPerson = new()
        {
            Id = person.Id,
            Name = _testPersons.CreateUniqueValue(),
            ShortName = _testPersons.CreateUniqueValue(),
            IdentityUserId = null
        };

        await _personRepository.UpdateAsync(changedPerson, TestContext.Current.CancellationToken);

        Person? storedPerson = await _testPersons.FindStoredAsync(person.Id);
        Assert.NotNull(storedPerson);
        Assert.Equal(changedPerson.Name, storedPerson.Name);
        Assert.Equal(changedPerson.ShortName, storedPerson.ShortName);
        Assert.Null(storedPerson.IdentityUserId);
    }

    [Fact]
    public async Task ThrowsPersonNotFoundException_WhenThePersonIsNotStored()
    {
        Person person = _testPersons.Create();

        PersonNotFoundException exception = await Assert.ThrowsAsync<PersonNotFoundException>(
            () => _personRepository.UpdateAsync(person, TestContext.Current.CancellationToken));

        Assert.Equal(person.Id, exception.PersonId);
    }

    [Fact]
    public async Task LogsAWarningWithThePersonId_WhenThePersonIsNotStored()
    {
        Person person = _testPersons.Create();

        await Assert.ThrowsAsync<PersonNotFoundException>(
            () => _personRepository.UpdateAsync(person, TestContext.Current.CancellationToken));

        Assert.Contains(
            _database.PersonRepositoryLogger.Entries,
            logEntry => (logEntry.LogLevel == LogLevel.Warning)
                && (logEntry.ContextData.GetValueOrDefault("PersonId") == person.Id.ToString()));
    }

    [Fact]
    public async Task ThrowsDuplicatePersonException_WhenTheNameIsUsedByAnotherPerson()
    {
        Person otherPerson = _testPersons.Create();
        Person person = _testPersons.Create();
        await _personRepository.InsertAsync(otherPerson, TestContext.Current.CancellationToken);
        await _personRepository.InsertAsync(person, TestContext.Current.CancellationToken);
        person.Name = otherPerson.Name;

        DuplicatePersonException exception = await Assert.ThrowsAsync<DuplicatePersonException>(
            () => _personRepository.UpdateAsync(person, TestContext.Current.CancellationToken));

        Assert.Equal(person.Id, exception.PersonId);
        Assert.Equal(nameof(Person.Name), exception.PropertyName);
    }

    [Fact]
    public async Task KeepsTheStoredPerson_WhenAValueIsUsedByAnotherPerson()
    {
        Person otherPerson = _testPersons.Create();
        Person person = _testPersons.Create();
        await _personRepository.InsertAsync(otherPerson, TestContext.Current.CancellationToken);
        await _personRepository.InsertAsync(person, TestContext.Current.CancellationToken);
        string originalShortName = person.ShortName;
        person.ShortName = otherPerson.ShortName;

        await Assert.ThrowsAsync<DuplicatePersonException>(
            () => _personRepository.UpdateAsync(person, TestContext.Current.CancellationToken));

        Assert.Equal(originalShortName, (await _testPersons.FindStoredAsync(person.Id))?.ShortName);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testPersons.DisposeAsync();
}
