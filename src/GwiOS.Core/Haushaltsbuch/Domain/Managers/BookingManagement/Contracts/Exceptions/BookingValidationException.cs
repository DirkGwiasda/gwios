namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when a booking is rejected because it violates at least one validation rule.
/// </summary>
/// <param name="bookingId">The ID of the rejected booking.</param>
/// <param name="validationErrors">One message per violated rule; never empty.</param>
public sealed class BookingValidationException(Guid bookingId, IReadOnlyList<string> validationErrors)
    : Exception($"Booking '{bookingId}' is invalid: {string.Join(" ", validationErrors)}")
{
    /// <summary>
    /// The ID of the rejected booking.
    /// </summary>
    public Guid BookingId { get; } = bookingId;

    /// <summary>
    /// One message per violated rule; never empty.
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; } = validationErrors;
}
