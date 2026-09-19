using RecurringItemNotFoundExceptionUnderTest =
    GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Exceptions.RecurringItemNotFoundException;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Exceptions.RecurringItemNotFoundException;

/// <summary>
/// Covers the constructor of <c>RecurringItemNotFoundException</c>.
/// </summary>
public sealed class ConstructorTests
{
    private readonly Guid _recurringItemId = Guid.CreateVersion7();

    [Fact]
    public void KeepsTheRecurringItemId()
    {
        RecurringItemNotFoundExceptionUnderTest exception = new(_recurringItemId);

        Assert.Equal(_recurringItemId, exception.RecurringItemId);
    }

    [Fact]
    public void NamesTheRecurringItemIdInTheMessage()
    {
        RecurringItemNotFoundExceptionUnderTest exception = new(_recurringItemId);

        Assert.Equal($"No recurring item with ID '{_recurringItemId}' is stored.", exception.Message);
    }
}
