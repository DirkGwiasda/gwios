using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;

/// <summary>
/// Manages the bookings of the Haushaltsbuch: creates, changes, reads and deletes them, and rejects bookings that
/// are invalid.
/// </summary>
public interface IBookingManager
{
    /// <summary>
    /// Validates and stores the given new booking and returns it. Throws a <see cref="BookingValidationException"/>
    /// if the booking is invalid.
    /// </summary>
    Task<Booking> CreateBookingAsync(Booking booking, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and stores the given changed booking. Throws a <see cref="BookingValidationException"/> if the
    /// booking is invalid, or a <see cref="BookingNotFoundException"/> if no booking with its ID is stored.
    /// </summary>
    Task UpdateBookingAsync(Booking booking, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all stored bookings with a date between <paramref name="fromDate"/> and <paramref name="toDate"/>,
    /// both inclusive, newest first. The list is empty if no booking falls in the range.
    /// </summary>
    Task<List<Booking>> GetBookingsInRangeAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the booking with the given ID. Does nothing if no such booking is stored.
    /// </summary>
    Task DeleteBookingAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
