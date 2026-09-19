using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;

/// <summary>
/// Stands in for <see cref="IBookingManager"/> for the workflow tests. It keeps bookings in memory and does not
/// validate.
/// </summary>
public sealed class BookingManagerFake : IBookingManager
{
    private readonly List<Booking> _bookings = [];

    /// <summary>
    /// All bookings created so far, in call order.
    /// </summary>
    public List<Booking> CreatedBookings { get; } = [];

    /// <summary>
    /// Stores a booking directly, as if it had been created before the test.
    /// </summary>
    public void Add(Booking booking)
        => _bookings.Add(booking);

    public Task<Booking> CreateBookingAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        _bookings.Add(booking);
        CreatedBookings.Add(booking);
        return Task.FromResult(booking);
    }

    public Task UpdateBookingAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        int index = _bookings.FindIndex(stored => stored.Id == booking.Id);
        _bookings[index] = booking;
        return Task.CompletedTask;
    }

    public Task<List<Booking>> GetBookingsInRangeAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
        => Task.FromResult(_bookings
            .Where(booking => (booking.Date >= fromDate) && (booking.Date <= toDate))
            .OrderByDescending(booking => booking.Date)
            .ThenBy(booking => booking.Id)
            .ToList());

    public Task DeleteBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        _bookings.RemoveAll(booking => booking.Id == bookingId);
        return Task.CompletedTask;
    }
}
