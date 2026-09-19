namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when an operation requires a stored recurring item, but no recurring item with the given ID is stored.
/// </summary>
/// <param name="recurringItemId">The ID that no stored recurring item has.</param>
public sealed class RecurringItemNotFoundException(Guid recurringItemId)
    : Exception($"No recurring item with ID '{recurringItemId}' is stored.")
{
    /// <summary>
    /// The ID that no stored recurring item has.
    /// </summary>
    public Guid RecurringItemId { get; } = recurringItemId;
}
