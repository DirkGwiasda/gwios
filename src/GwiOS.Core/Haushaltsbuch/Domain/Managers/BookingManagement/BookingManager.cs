using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement;

internal sealed class BookingManager(
    IBookingRepository bookingRepository,
    IBookingValidator bookingValidator,
    ILogger<BookingManager> logger) : IBookingManager
{
    private readonly IBookingRepository _bookingRepository = bookingRepository;
    private readonly IBookingValidator _bookingValidator = bookingValidator;
    private readonly ILogger<BookingManager> _logger = logger;

    public async Task<Booking> CreateBookingAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        ThrowIfInvalid(booking);
        await _bookingRepository.InsertAsync(booking, cancellationToken);
        _logger.LogInformation("Booking created.", CreateBookingIdData(booking.Id));
        return booking;
    }

    public async Task UpdateBookingAsync(Booking booking, CancellationToken cancellationToken = default)
    {
        ThrowIfInvalid(booking);
        await _bookingRepository.UpdateAsync(booking, cancellationToken);
        _logger.LogInformation("Booking updated.", CreateBookingIdData(booking.Id));
    }

    public async Task<List<Booking>> GetBookingsInRangeAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
        => await _bookingRepository.GetInRangeAsync(fromDate, toDate, cancellationToken);

    public async Task DeleteBookingAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        await _bookingRepository.DeleteAsync(bookingId, cancellationToken);
        _logger.LogInformation("Booking deleted.", CreateBookingIdData(bookingId));
    }

    private void ThrowIfInvalid(Booking booking)
    {
        if (!_bookingValidator.IsValid(booking, out List<string> validationErrors))
        {
            throw new BookingValidationException(booking.Id, validationErrors);
        }
    }

    private static Dictionary<string, string> CreateBookingIdData(Guid bookingId)
        => new() { ["BookingId"] = bookingId.ToString() };
}
