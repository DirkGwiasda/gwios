using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;

/// <summary>
/// Stands in for <see cref="IBookingRepository"/> by keeping bookings in memory. It filters, updates and deletes
/// like the real repository.
/// </summary>
public sealed class BookingRepositoryFake : IBookingRepository
{
    private readonly List<Booking> _bookings = [];

    public Task EnsureStorageCreatedAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task InsertAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        _bookings.Add(booking);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        int index = _bookings.FindIndex(stored => stored.Id == booking.Id);
        if (index < 0)
        {
            throw new BookingNotFoundException(booking.Id);
        }

        _bookings[index] = booking;
        return Task.CompletedTask;
    }

    public Task<List<Booking>> GetInRangeAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
        => Task.FromResult(_bookings
            .Where(booking => (booking.Date >= fromDate) && (booking.Date <= toDate))
            .OrderByDescending(booking => booking.Date)
            .ThenBy(booking => booking.Id)
            .ToList());

    public Task DeleteAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        _bookings.RemoveAll(booking => booking.Id == bookingId);
        return Task.CompletedTask;
    }
}
