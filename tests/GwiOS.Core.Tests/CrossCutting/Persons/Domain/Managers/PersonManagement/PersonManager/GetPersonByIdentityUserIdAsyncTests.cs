using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;

namespace GwiOS.Core.Tests.CrossCutting.Persons.Domain.Managers.PersonManagement.PersonManager;

/// <summary>
/// Covers <c>PersonManager.GetPersonByIdentityUserIdAsync</c>.
/// </summary>
public sealed class GetPersonByIdentityUserIdAsyncTests
{
    private readonly PersonManagerProbe _probe = new();
    private readonly Person _anna = new() { Name = "Anna", ShortName = "A", IdentityUserId = "user-1" };
    private readonly Person _bernd = new() { Name = "Bernd", ShortName = "B", IdentityUserId = "user-2" };

    [Fact]
    public async Task ReturnsThePersonLinkedToTheIdentityUser()
    {
        await _probe.PersonRepository.InsertAsync(_anna, TestContext.Current.CancellationToken);
        await _probe.PersonRepository.InsertAsync(_bernd, TestContext.Current.CancellationToken);

        Person? person =
            await _probe.PersonManager.GetPersonByIdentityUserIdAsync("user-1", TestContext.Current.CancellationToken);

        Assert.Same(_anna, person);
    }

    [Fact]
    public async Task ReturnsNull_WhenNoPersonIsLinkedToTheIdentityUser()
    {
        await _probe.PersonRepository.InsertAsync(_bernd, TestContext.Current.CancellationToken);

        Person? person =
            await _probe.PersonManager.GetPersonByIdentityUserIdAsync("user-1", TestContext.Current.CancellationToken);

        Assert.Null(person);
    }
}
