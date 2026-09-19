namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;

/// <summary>
/// Whether a booking in the Haushaltsbuch has actually happened yet.
/// </summary>
public enum BookingStatus
{
    /// <summary>
    /// The money has actually moved.
    /// </summary>
    Booked,

    /// <summary>
    /// The booking is expected but has not happened yet.
    /// </summary>
    Pending
}
