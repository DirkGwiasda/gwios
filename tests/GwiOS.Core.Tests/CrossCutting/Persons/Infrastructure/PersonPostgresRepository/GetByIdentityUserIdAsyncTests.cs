using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;
using GwiOS.Core.Tests.TestInfrastructure;

namespace GwiOS.Core.Tests.CrossCutting.Persons.Infrastructure.PersonPostgresRepository;

/// <summary>
/// Covers <c>PersonPostgresRepository.GetByIdentityUserIdAsync</c> against the PostgreSQL test database.
/// </summary>
[Collection(PostgresTestDatabaseCollection.Name)]
public sealed class GetByIdentityUserIdAsyncTests(PostgresTestDatabase database) : IAsyncLifetime
{
    private readonly IPersonRepository _personRepository = database.PersonRepository;
    private readonly TestPersons _testPersons = new(database.PersonRepository);

    [Fact]
    public async Task ReturnsThePersonLinkedToTheIdentityUser()
    {
        Person person = _testPersons.Create();
        await _personRepository.InsertAsync(person, TestContext.Current.CancellationToken);
        await _personRepository.InsertAsync(_testPersons.Create(), TestContext.Current.CancellationToken);

        Person? foundPerson = await _personRepository.GetByIdentityUserIdAsync(
            person.IdentityUserId!,
            TestContext.Current.CancellationToken);

        Assert.NotNull(foundPerson);
        Assert.Equal(person.Id, foundPerson.Id);
        Assert.Equal(person.Name, foundPerson.Name);
        Assert.Equal(person.ShortName, foundPerson.ShortName);
        Assert.Equal(person.IdentityUserId, foundPerson.IdentityUserId);
    }

    [Fact]
    public async Task ReturnsNull_WhenNoPersonIsLinkedToTheIdentityUser()
    {
        Person? foundPerson = await _personRepository.GetByIdentityUserIdAsync(
            _testPersons.CreateUniqueValue(),
            TestContext.Current.CancellationToken);

        Assert.Null(foundPerson);
    }

    public ValueTask InitializeAsync()
        => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
        => _testPersons.DisposeAsync();
}
