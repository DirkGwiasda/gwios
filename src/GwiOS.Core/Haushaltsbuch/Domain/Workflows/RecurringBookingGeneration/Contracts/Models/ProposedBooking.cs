using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringBookingGeneration.Contracts.Models;

/// <summary>
/// A single occurrence of an active recurring item, proposed as a booking for a reviewed month but not yet
/// confirmed.
/// </summary>
public class ProposedBooking
{
    /// <summary>
    /// The ID of the recurring item this occurrence was proposed from.
    /// </summary>
    public required Guid RecurringItemId { get; set; }

    /// <summary>
    /// The name of the recurring item, to show in the review popup.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Whether the occurrence is income or an expense.
    /// </summary>
    public required CategoryType Type { get; set; }

    /// <summary>
    /// The ID of the category the resulting booking would be booked to.
    /// </summary>
    public required Guid CategoryId { get; set; }

    /// <summary>
    /// The date the occurrence falls due.
    /// </summary>
    public required DateOnly Date { get; set; }

    /// <summary>
    /// The amount of the occurrence, always positive regardless of <see cref="Type"/>. In euros.
    /// </summary>
    public required decimal Amount { get; set; }
}
