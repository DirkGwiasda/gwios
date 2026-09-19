using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;

/// <summary>
/// Stands in for <see cref="IRecurringItemValidator"/>. It considers every recurring item valid until a test sets
/// <see cref="ValidationErrors"/>, and records every recurring item it validates.
/// </summary>
public sealed class RecurringItemValidatorFake : IRecurringItemValidator
{
    private readonly List<RecurringItem> _validatedRecurringItems = [];

    /// <summary>
    /// The validation errors reported for every recurring item. Empty by default, which makes every recurring item
    /// valid.
    /// </summary>
    public List<string> ValidationErrors { get; set; } = [];

    /// <summary>
    /// All recurring items validated so far, in the order of validation.
    /// </summary>
    public IReadOnlyList<RecurringItem> ValidatedRecurringItems
        => _validatedRecurringItems;

    public bool IsValid(RecurringItem recurringItem, out List<string> validationErrors)
    {
        _validatedRecurringItems.Add(recurringItem);
        validationErrors = [.. ValidationErrors];
        return validationErrors.Count == 0;
    }
}
