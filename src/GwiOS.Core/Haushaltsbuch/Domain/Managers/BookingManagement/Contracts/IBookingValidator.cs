using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;

/// <summary>
/// Checks the values of a single booking.
/// </summary>
public interface IBookingValidator
{
    /// <summary>
    /// Returns whether the given booking is valid. <paramref name="validationErrors"/> receives one message per
    /// violated rule; it is empty if the booking is valid.
    /// </summary>
    bool IsValid(Booking booking, out List<string> validationErrors);
}
