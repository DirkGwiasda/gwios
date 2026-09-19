using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;

/// <summary>
/// Persists, reads and deletes bookings in the underlying storage.
/// </summary>
public interface IBookingRepository
{
    /// <summary>
    /// Creates the tables required for bookings unless they already exist. The database itself must already exist.
    /// The operation is idempotent: calling it against existing tables changes nothing.
    /// </summary>
    Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores the given new booking.
    /// </summary>
    Task InsertAsync(Booking booking, CancellationToken cancellationToken = default);

    /// <summary>
    /// Overwrites the stored booking with the ID of the given booking. Throws a <see cref="BookingNotFoundException"/>
    /// if no such booking is stored.
    /// </summary>
    Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all stored bookings with a date between <paramref name="fromDate"/> and <paramref name="toDate"/>,
    /// both inclusive, newest first. The list is empty if no booking falls in the range.
    /// </summary>
    Task<List<Booking>> GetInRangeAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the booking with the given ID. Does nothing if no such booking is stored.
    /// </summary>
    Task DeleteAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
