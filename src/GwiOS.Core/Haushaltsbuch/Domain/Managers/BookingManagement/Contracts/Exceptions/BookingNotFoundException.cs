namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when an operation requires a stored booking, but no booking with the given ID is stored.
/// </summary>
/// <param name="bookingId">The ID that no stored booking has.</param>
public sealed class BookingNotFoundException(Guid bookingId)
    : Exception($"No booking with ID '{bookingId}' is stored.")
{
    /// <summary>
    /// The ID that no stored booking has.
    /// </summary>
    public Guid BookingId { get; } = bookingId;
}
