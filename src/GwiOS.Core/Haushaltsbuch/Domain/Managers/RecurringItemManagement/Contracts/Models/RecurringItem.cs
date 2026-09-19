using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;

/// <summary>
/// A recurring income or expense in the Haushaltsbuch (e.g. rent or a salary) that is proposed as a booking
/// whenever it falls due.
/// </summary>
public class RecurringItem
{
    /// <summary>
    /// The unique identifier of the recurring item. Defaults to a new time-ordered UUID (version 7).
    /// </summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>
    /// The name of the recurring item (e.g. "Miete"). Must not be empty.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Whether the recurring item is income or an expense.
    /// </summary>
    public required CategoryType Type { get; set; }

    /// <summary>
    /// The ID of the category the recurring item is booked to.
    /// </summary>
    public required Guid CategoryId { get; set; }

    /// <summary>
    /// The amount due on each occurrence, always positive regardless of <see cref="Type"/>. In euros.
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    /// How often the item falls due.
    /// </summary>
    public required RecurringInterval Interval { get; set; }

    /// <summary>
    /// The day of the month the item falls due, from 1 to 28. Meaningless for <see cref="RecurringInterval.Weekly"/>,
    /// where it always falls due on the 1st, 8th, 15th and 22nd. Defaults to 1.
    /// </summary>
    public int DueDay { get; set; } = 1;

    /// <summary>
    /// The month (0 = January … 11 = December) the item first falls due, used to phase <see cref="RecurringInterval.Quarterly"/>
    /// and <see cref="RecurringInterval.Yearly"/> items. Defaults to 0.
    /// </summary>
    public int StartMonth { get; set; }

    /// <summary>
    /// Whether the item is active. An inactive item is not proposed as a booking. Defaults to <c>true</c>.
    /// </summary>
    public bool Active { get; set; } = true;
}
