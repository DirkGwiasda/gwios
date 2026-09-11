using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.CrossCutting.Persons.Infrastructure.PersonPostgresRepository;

/// <summary>
/// Covers <c>PersonPostgresRepository.DeleteAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class DeleteAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly IPersonRepository _personRepository = database.PersonRepository;
    private readonly TestPersons _testPersons = new(database.PersonRepository);

    [Fact]
    public async Task DeletesThePersonWithTheGivenId()
    {
        Person person = _testPersons.Create();
        await _personRepository.InsertAsync(person, TestContext.Current.CancellationToken);

        await _personRepository.DeleteAsync(person.Id, TestContext.Current.CancellationToken);

        Assert.Null(await _testPersons.FindStoredAsync(person.Id));
    }

    [Fact]
    public async Task KeepsOtherPersons()
    {
        Person person = _testPersons.Create();
        Person otherPerson = _testPersons.Create();
        await _personRepository.InsertAsync(person, TestContext.Current.CancellationToken);
        await _personRepository.InsertAsync(otherPerson, TestContext.Current.CancellationToken);

        await _personRepository.DeleteAsync(person.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(await _testPersons.FindStoredAsync(otherPerson.Id));
    }

    [Fact]
    public async Task Succeeds_WhenThePersonIsNotStored()
    {
        Exception? exception = await Record.ExceptionAsync(
            () => _personRepository.DeleteAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken));

        Assert.Null(exception);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testPersons.DisposeAsync();
}
