namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts.Models;

/// <summary>
/// Marks a month of the Haushaltsbuch as reviewed: someone has seen the recurring items proposed for it and either
/// confirmed or declined each one, so the review popup will not ask again for that month.
/// </summary>
public class MonthReview
{
    /// <summary>
    /// The first day of the reviewed month. Must be the 1st of its month.
    /// </summary>
    public required DateOnly Month { get; set; }

    /// <summary>
    /// The point in time the month was reviewed, in UTC. Defaults to the current UTC time.
    /// </summary>
    public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;
}
