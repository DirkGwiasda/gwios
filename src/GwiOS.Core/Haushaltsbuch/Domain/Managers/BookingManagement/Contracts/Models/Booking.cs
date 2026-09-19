using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;

/// <summary>
/// A single income or expense booked (or expected) on a given date in the Haushaltsbuch.
/// </summary>
public class Booking
{
    /// <summary>
    /// The unique identifier of the booking. Defaults to a new time-ordered UUID (version 7).
    /// </summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>
    /// The date the booking falls on.
    /// </summary>
    public required DateOnly Date { get; set; }

    /// <summary>
    /// Whether the booking is income or an expense.
    /// </summary>
    public required CategoryType Type { get; set; }

    /// <summary>
    /// The ID of the category the booking is booked to.
    /// </summary>
    public required Guid CategoryId { get; set; }

    /// <summary>
    /// A free-text description of the booking, or <c>null</c> if it has none.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The amount of the booking, always positive regardless of <see cref="Type"/>. In euros.
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    /// Whether the booking has actually happened yet.
    /// </summary>
    public required BookingStatus Status { get; set; }

    /// <summary>
    /// The ID of the recurring item this booking was generated from, or <c>null</c> if it was entered directly.
    /// Purely informational: the booking still exists if the recurring item it points to is later deleted.
    /// </summary>
    public Guid? RecurringItemId { get; set; }
}
