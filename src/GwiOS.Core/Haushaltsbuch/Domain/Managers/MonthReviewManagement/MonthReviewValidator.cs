using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement;

internal sealed class MonthReviewValidator(ILogger<MonthReviewValidator> logger) : IMonthReviewValidator
{
    private readonly ILogger<MonthReviewValidator> _logger = logger;

    public bool IsValid(MonthReview monthReview, out List<string> validationErrors)
    {
        validationErrors = CollectValidationErrors(monthReview);
        if (validationErrors.Count == 0)
        {
            return true;
        }

        LogValidationErrors(monthReview, validationErrors);
        return false;
    }

    private static List<string> CollectValidationErrors(MonthReview monthReview)
    {
        List<string> validationErrors = [];

        if (monthReview.Month.Day != 1)
        {
            validationErrors.Add("The month must be given as the 1st of its month.");
        }

        return validationErrors;
    }

    private void LogValidationErrors(MonthReview monthReview, List<string> validationErrors)
        => _logger.LogWarning(
            "Month review is invalid.",
            new Dictionary<string, string>
            {
                ["Month"] = monthReview.Month.ToString("O"),
                ["ValidationErrors"] = string.Join(" ", validationErrors)
            });
}
