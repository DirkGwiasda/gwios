using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.CrossCutting.Persons.Infrastructure.PersonPostgresRepository;

/// <summary>
/// Covers <c>PersonPostgresRepository.InsertAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class InsertAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly PostgresTestDatabase _database = database;
    private readonly IPersonRepository _personRepository = database.PersonRepository;
    private readonly TestPersons _testPersons = new(database.PersonRepository);

    [Fact]
    public async Task StoresAllPropertiesOfThePerson()
    {
        Person person = _testPersons.Create();

        await _personRepository.InsertAsync(person, TestContext.Current.CancellationToken);

        Person? storedPerson = await _testPersons.FindStoredAsync(person.Id);
        Assert.NotNull(storedPerson);
        Assert.Equal(person.Name, storedPerson.Name);
        Assert.Equal(person.ShortName, storedPerson.ShortName);
        Assert.Equal(person.IdentityUserId, storedPerson.IdentityUserId);
    }

    [Fact]
    public async Task StoresSeveralPersonsWithoutIdentityUserId()
    {
        Person firstPerson = _testPersons.Create();
        firstPerson.IdentityUserId = null;
        Person secondPerson = _testPersons.Create();
        secondPerson.IdentityUserId = null;

        await _personRepository.InsertAsync(firstPerson, TestContext.Current.CancellationToken);
        await _personRepository.InsertAsync(secondPerson, TestContext.Current.CancellationToken);

        Person? firstStoredPerson = await _testPersons.FindStoredAsync(firstPerson.Id);
        Person? secondStoredPerson = await _testPersons.FindStoredAsync(secondPerson.Id);
        Assert.NotNull(firstStoredPerson);
        Assert.NotNull(secondStoredPerson);
        Assert.Null(firstStoredPerson.IdentityUserId);
        Assert.Null(secondStoredPerson.IdentityUserId);
    }

    [Fact]
    public async Task ThrowsDuplicatePersonException_WhenTheIdIsAlreadyUsed()
    {
        Person storedPerson = _testPersons.Create();
        await _personRepository.InsertAsync(storedPerson, TestContext.Current.CancellationToken);
        Person person = new()
        {
            Id = storedPerson.Id,
            Name = _testPersons.CreateUniqueValue(),
            ShortName = _testPersons.CreateUniqueValue()
        };

        await AssertInsertThrowsDuplicatePersonExceptionAsync(person, nameof(Person.Id));
    }

    [Fact]
    public async Task ThrowsDuplicatePersonException_WhenTheNameIsAlreadyUsed()
    {
        Person storedPerson = _testPersons.Create();
        await _personRepository.InsertAsync(storedPerson, TestContext.Current.CancellationToken);
        Person person = _testPersons.Create();
        person.Name = storedPerson.Name;

        await AssertInsertThrowsDuplicatePersonExceptionAsync(person, nameof(Person.Name));
    }

    [Fact]
    public async Task ThrowsDuplicatePersonException_WhenTheShortNameIsAlreadyUsed()
    {
        Person storedPerson = _testPersons.Create();
        await _personRepository.InsertAsync(storedPerson, TestContext.Current.CancellationToken);
        Person person = _testPersons.Create();
        person.ShortName = storedPerson.ShortName;

        await AssertInsertThrowsDuplicatePersonExceptionAsync(person, nameof(Person.ShortName));
    }

    [Fact]
    public async Task ThrowsDuplicatePersonException_WhenTheIdentityUserIdIsAlreadyUsed()
    {
        Person storedPerson = _testPersons.Create();
        await _personRepository.InsertAsync(storedPerson, TestContext.Current.CancellationToken);
        Person person = _testPersons.Create();
        person.IdentityUserId = storedPerson.IdentityUserId;

        await AssertInsertThrowsDuplicatePersonExceptionAsync(person, nameof(Person.IdentityUserId));
    }

    [Fact]
    public async Task DoesNotStoreThePerson_WhenAValueIsAlreadyUsed()
    {
        Person storedPerson = _testPersons.Create();
        await _personRepository.InsertAsync(storedPerson, TestContext.Current.CancellationToken);
        Person person = _testPersons.Create();
        person.Name = storedPerson.Name;

        await Assert.ThrowsAsync<DuplicatePersonException>(
            () => _personRepository.InsertAsync(person, TestContext.Current.CancellationToken));

        Assert.Null(await _testPersons.FindStoredAsync(person.Id));
    }

    [Fact]
    public async Task LogsAWarningWithThePersonId_WhenAValueIsAlreadyUsed()
    {
        Person storedPerson = _testPersons.Create();
        await _personRepository.InsertAsync(storedPerson, TestContext.Current.CancellationToken);
        Person person = _testPersons.Create();
        person.Name = storedPerson.Name;

        await Assert.ThrowsAsync<DuplicatePersonException>(
            () => _personRepository.InsertAsync(person, TestContext.Current.CancellationToken));

        Assert.Contains(
            _database.PersonRepositoryLogger.Entries,
            logEntry => (logEntry.LogLevel == LogLevel.Warning)
                && (logEntry.ContextData.GetValueOrDefault("PersonId") == person.Id.ToString()));
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testPersons.DisposeAsync();

    private async Task AssertInsertThrowsDuplicatePersonExceptionAsync(Person person, string expectedPropertyName)
    {
        DuplicatePersonException exception = await Assert.ThrowsAsync<DuplicatePersonException>(
            () => _personRepository.InsertAsync(person, TestContext.Current.CancellationToken));

        Assert.Equal(person.Id, exception.PersonId);
        Assert.Equal(expectedPropertyName, exception.PropertyName);
    }
}
