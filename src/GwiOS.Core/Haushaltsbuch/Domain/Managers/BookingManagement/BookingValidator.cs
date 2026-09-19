using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement;

internal sealed class BookingValidator(ILogger<BookingValidator> logger) : IBookingValidator
{
    private readonly ILogger<BookingValidator> _logger = logger;

    public bool IsValid(Booking booking, out List<string> validationErrors)
    {
        validationErrors = CollectValidationErrors(booking);
        if (validationErrors.Count == 0)
        {
            return true;
        }

        LogValidationErrors(booking, validationErrors);
        return false;
    }

    private static List<string> CollectValidationErrors(Booking booking)
    {
        List<string> validationErrors = [];

        if (!Enum.IsDefined(booking.Type))
        {
            validationErrors.Add("The type is not a known category type.");
        }

        if (booking.CategoryId == Guid.Empty)
        {
            validationErrors.Add("The category is required.");
        }

        if (booking.Amount <= 0)
        {
            validationErrors.Add("The amount must be greater than 0.");
        }

        if (!Enum.IsDefined(booking.Status))
        {
            validationErrors.Add("The status is not a known booking status.");
        }

        return validationErrors;
    }

    private void LogValidationErrors(Booking booking, List<string> validationErrors)
        => _logger.LogWarning(
            "Booking is invalid.",
            new Dictionary<string, string>
            {
                ["BookingId"] = booking.Id.ToString(),
                ["ValidationErrors"] = string.Join(" ", validationErrors)
            });
}
