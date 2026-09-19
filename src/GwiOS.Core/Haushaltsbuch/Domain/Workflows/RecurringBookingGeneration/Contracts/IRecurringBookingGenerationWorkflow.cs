using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringBookingGeneration.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringBookingGeneration.Contracts;

/// <summary>
/// Proposes bookings for the active recurring items due in a month, and turns the confirmed proposals into
/// bookings.
/// </summary>
public interface IRecurringBookingGenerationWorkflow
{
    /// <summary>
    /// Returns whether the given month still needs review: nobody has confirmed or declined its proposed bookings
    /// yet. <paramref name="month"/> may be any day of the month; only its year and month are used.
    /// </summary>
    Task<bool> NeedsReviewAsync(DateOnly month, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the occurrences of active recurring items due in the given month that are not yet booked.
    /// <paramref name="month"/> may be any day of the month; only its year and month are used.
    /// </summary>
    Task<List<ProposedBooking>> GetProposedBookingsAsync(DateOnly month, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a pending booking for each of the given proposed bookings and marks the month as reviewed, whether
    /// or not any proposals were selected. <paramref name="month"/> may be any day of the month; only its year and
    /// month are used.
    /// </summary>
    Task ConfirmAsync(
        DateOnly month,
        IReadOnlyList<ProposedBooking> selectedBookings,
        CancellationToken cancellationToken = default);
}
