using PersonNotFoundExceptionUnderTest =
    GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions.PersonNotFoundException;

namespace GwiOS.Core.Tests.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions.PersonNotFoundException;

/// <summary>
/// Covers the constructor of <c>PersonNotFoundException</c>.
/// </summary>
public sealed class ConstructorTests
{
    private readonly Guid _personId = Guid.CreateVersion7();

    [Fact]
    public void KeepsThePersonId()
    {
        PersonNotFoundExceptionUnderTest exception = new(_personId);

        Assert.Equal(_personId, exception.PersonId);
    }

    [Fact]
    public void NamesThePersonIdInTheMessage()
    {
        PersonNotFoundExceptionUnderTest exception = new(_personId);

        Assert.Equal($"No person with ID '{_personId}' is stored.", exception.Message);
    }
}
