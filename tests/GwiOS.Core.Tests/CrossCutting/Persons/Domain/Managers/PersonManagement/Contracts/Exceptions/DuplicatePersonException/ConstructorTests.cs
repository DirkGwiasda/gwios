using DuplicatePersonExceptionUnderTest =
    GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions.DuplicatePersonException;

namespace GwiOS.Core.Tests.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions.DuplicatePersonException;

/// <summary>
/// Covers the constructor of <c>DuplicatePersonException</c>.
/// </summary>
public sealed class ConstructorTests
{
    private readonly Guid _personId = Guid.CreateVersion7();
    private readonly InvalidOperationException _innerException = new("Storage error.");

    [Fact]
    public void KeepsThePersonIdThePropertyNameAndTheInnerException()
    {
        DuplicatePersonExceptionUnderTest exception = new(_personId, "ShortName", _innerException);

        Assert.Equal(_personId, exception.PersonId);
        Assert.Equal("ShortName", exception.PropertyName);
        Assert.Same(_innerException, exception.InnerException);
    }

    [Fact]
    public void NamesThePersonIdAndThePropertyInTheMessage()
    {
        DuplicatePersonExceptionUnderTest exception = new(_personId, "ShortName", _innerException);

        Assert.Equal(
            $"Person '{_personId}' cannot be stored because its ShortName is already used by another person.",
            exception.Message);
    }
}
