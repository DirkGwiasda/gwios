using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement;

internal sealed class RecurringItemValidator(ILogger<RecurringItemValidator> logger) : IRecurringItemValidator
{
    private const int MinDueDay = 1;
    private const int MaxDueDay = 28;
    private const int MinStartMonth = 0;
    private const int MaxStartMonth = 11;

    private readonly ILogger<RecurringItemValidator> _logger = logger;

    public bool IsValid(RecurringItem recurringItem, out List<string> validationErrors)
    {
        validationErrors = CollectValidationErrors(recurringItem);
        if (validationErrors.Count == 0)
        {
            return true;
        }

        LogValidationErrors(recurringItem, validationErrors);
        return false;
    }

    private static List<string> CollectValidationErrors(RecurringItem recurringItem)
    {
        List<string> validationErrors = [];

        if (string.IsNullOrWhiteSpace(recurringItem.Name))
        {
            validationErrors.Add("The name must not be empty.");
        }

        if (!Enum.IsDefined(recurringItem.Type))
        {
            validationErrors.Add("The type is not a known category type.");
        }

        if (recurringItem.CategoryId == Guid.Empty)
        {
            validationErrors.Add("The category is required.");
        }

        if (recurringItem.Amount <= 0)
        {
            validationErrors.Add("The amount must be greater than 0.");
        }

        if (!Enum.IsDefined(recurringItem.Interval))
        {
            validationErrors.Add("The interval is not a known recurring interval.");
        }

        if ((recurringItem.DueDay < MinDueDay) || (recurringItem.DueDay > MaxDueDay))
        {
            validationErrors.Add($"The due day must be between {MinDueDay} and {MaxDueDay}.");
        }

        if ((recurringItem.StartMonth < MinStartMonth) || (recurringItem.StartMonth > MaxStartMonth))
        {
            validationErrors.Add($"The start month must be between {MinStartMonth} and {MaxStartMonth}.");
        }

        return validationErrors;
    }

    private void LogValidationErrors(RecurringItem recurringItem, List<string> validationErrors)
        => _logger.LogWarning(
            "Recurring item is invalid.",
            new Dictionary<string, string>
            {
                ["RecurringItemId"] = recurringItem.Id.ToString(),
                ["ValidationErrors"] = string.Join(" ", validationErrors)
            });
}
