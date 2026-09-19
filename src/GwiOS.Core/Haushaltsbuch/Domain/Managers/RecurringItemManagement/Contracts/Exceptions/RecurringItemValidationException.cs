namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when a recurring item is rejected because it violates at least one validation rule.
/// </summary>
/// <param name="recurringItemId">The ID of the rejected recurring item.</param>
/// <param name="validationErrors">One message per violated rule; never empty.</param>
public sealed class RecurringItemValidationException(Guid recurringItemId, IReadOnlyList<string> validationErrors)
    : Exception($"Recurring item '{recurringItemId}' is invalid: {string.Join(" ", validationErrors)}")
{
    /// <summary>
    /// The ID of the rejected recurring item.
    /// </summary>
    public Guid RecurringItemId { get; } = recurringItemId;

    /// <summary>
    /// One message per violated rule; never empty.
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; } = validationErrors;
}
