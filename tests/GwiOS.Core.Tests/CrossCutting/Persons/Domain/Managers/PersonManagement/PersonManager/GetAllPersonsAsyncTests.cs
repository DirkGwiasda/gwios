using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;

namespace GwiOS.Core.Tests.CrossCutting.Persons.Domain.Managers.PersonManagement.PersonManager;

/// <summary>
/// Covers <c>PersonManager.GetAllPersonsAsync</c>.
/// </summary>
public sealed class GetAllPersonsAsyncTests
{
    private readonly PersonManagerProbe _probe = new();

    [Fact]
    public async Task ReturnsThePersonsOfTheRepositoryInItsOrder()
    {
        Person bernd = new() { Name = "Bernd", ShortName = "B" };
        Person anna = new() { Name = "Anna", ShortName = "A" };
        await _probe.PersonRepository.InsertAsync(bernd, TestContext.Current.CancellationToken);
        await _probe.PersonRepository.InsertAsync(anna, TestContext.Current.CancellationToken);

        List<Person> persons = await _probe.PersonManager.GetAllPersonsAsync(TestContext.Current.CancellationToken);

        Assert.Equal([anna, bernd], persons);
    }

    [Fact]
    public async Task ReturnsAnEmptyList_WhenNoPersonsAreStored()
    {
        List<Person> persons = await _probe.PersonManager.GetAllPersonsAsync(TestContext.Current.CancellationToken);

        Assert.Empty(persons);
    }
}
