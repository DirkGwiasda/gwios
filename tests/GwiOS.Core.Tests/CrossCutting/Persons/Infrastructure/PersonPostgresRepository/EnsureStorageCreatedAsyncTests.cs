using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.CrossCutting.Persons.Infrastructure.PersonPostgresRepository;

/// <summary>
/// Covers <c>PersonPostgresRepository.EnsureStorageCreatedAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class EnsureStorageCreatedAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly IPersonRepository _personRepository = database.PersonRepository;
    private readonly TestPersons _testPersons = new(database.PersonRepository);

    [Fact]
    public async Task KeepsExistingPersons_WhenTheTablesAlreadyExist()
    {
        Person person = _testPersons.Create();
        await _personRepository.InsertAsync(person, TestContext.Current.CancellationToken);

        await _personRepository.EnsureStorageCreatedAsync(TestContext.Current.CancellationToken);

        Assert.NotNull(await _testPersons.FindStoredAsync(person.Id));
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testPersons.DisposeAsync();
}
