using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;

/// <summary>
/// Stands in for <see cref="IBookingValidator"/>. It considers every booking valid until a test sets
/// <see cref="ValidationErrors"/>, and records every booking it validates.
/// </summary>
public sealed class BookingValidatorFake : IBookingValidator
{
    private readonly List<Booking> _validatedBookings = [];

    /// <summary>
    /// The validation errors reported for every booking. Empty by default, which makes every booking valid.
    /// </summary>
    public List<string> ValidationErrors { get; set; } = [];

    /// <summary>
    /// All bookings validated so far, in the order of validation.
    /// </summary>
    public IReadOnlyList<Booking> ValidatedBookings
        => _validatedBookings;

    public bool IsValid(Booking booking, out List<string> validationErrors)
    {
        _validatedBookings.Add(booking);
        validationErrors = [.. ValidationErrors];
        return validationErrors.Count == 0;
    }
}
