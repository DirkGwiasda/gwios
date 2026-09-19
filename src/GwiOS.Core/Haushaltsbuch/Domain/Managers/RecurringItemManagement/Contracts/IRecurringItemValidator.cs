using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;

/// <summary>
/// Checks the values of a single recurring item.
/// </summary>
public interface IRecurringItemValidator
{
    /// <summary>
    /// Returns whether the given recurring item is valid. <paramref name="validationErrors"/> receives one message
    /// per violated rule; it is empty if the recurring item is valid.
    /// </summary>
    bool IsValid(RecurringItem recurringItem, out List<string> validationErrors);
}
