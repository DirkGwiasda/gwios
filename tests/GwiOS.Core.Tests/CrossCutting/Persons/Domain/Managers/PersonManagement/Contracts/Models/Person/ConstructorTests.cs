using PersonUnderTest = GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models.Person;

namespace GwiOS.Core.Tests.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models.Person;

/// <summary>
/// Covers the construction of <c>Person</c> and the defaults it assigns.
/// </summary>
public sealed class ConstructorTests
{
    [Fact]
    public void AssignsAVersion7Id()
    {
        PersonUnderTest person = new() { Name = "Anna", ShortName = "A" };

        Assert.Equal(7, person.Id.Version);
    }

    [Fact]
    public void AssignsADifferentIdToEachPerson()
    {
        PersonUnderTest firstPerson = new() { Name = "Anna", ShortName = "A" };
        PersonUnderTest secondPerson = new() { Name = "Anna", ShortName = "A" };

        Assert.NotEqual(firstPerson.Id, secondPerson.Id);
    }

    [Fact]
    public void LeavesTheIdentityUserIdUnset()
    {
        PersonUnderTest person = new() { Name = "Anna", ShortName = "A" };

        Assert.Null(person.IdentityUserId);
    }
}
