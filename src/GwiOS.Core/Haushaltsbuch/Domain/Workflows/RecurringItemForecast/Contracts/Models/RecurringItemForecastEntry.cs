namespace GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts.Models;

/// <summary>
/// One due date of a recurring item in a forecast.
/// </summary>
public sealed class RecurringItemForecastEntry
{
    /// <summary>
    /// The ID of the recurring item that falls due.
    /// </summary>
    public required Guid RecurringItemId { get; init; }

    /// <summary>
    /// The day the item falls due.
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// The name of the recurring item.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The ID of the category of the recurring item.
    /// </summary>
    public required Guid CategoryId { get; init; }

    /// <summary>
    /// The names of the category and its parents, from the root category down to the category itself; empty if the
    /// category is unknown.
    /// </summary>
    public required IReadOnlyList<string> CategoryPath { get; init; }

    /// <summary>
    /// The amount due, always positive; whether it is income or an expense follows from the list it is in.
    /// </summary>
    public required decimal Amount { get; init; }
}
