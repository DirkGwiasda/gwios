namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

/// <summary>
/// A category that a booking or recurring item in the Haushaltsbuch belongs to, either for income or for expenses.
/// Categories form a tree: a root category has no parent, every other category has exactly one.
/// </summary>
public class Category
{
    /// <summary>
    /// The unique identifier of the category. Defaults to a new time-ordered UUID (version 7).
    /// </summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>
    /// The name of the category. Must not be empty and must be unique among its sibling categories (the other
    /// categories with the same <see cref="ParentId"/>).
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Whether the category groups income or expenses. Must match the type of its parent category, if it has one.
    /// </summary>
    public required CategoryType Type { get; set; }

    /// <summary>
    /// The ID of the parent category, or <c>null</c> if this is a root category. Categories may be nested up to 5
    /// levels deep, root categories counting as level 1.
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// The display color of the category as a hex code (e.g. <c>#3399ff</c>). Required for a root category
    /// (<see cref="ParentId"/> is <c>null</c>); a sub-category is always <c>null</c> here and shows the color of its
    /// root category instead.
    /// </summary>
    public string? Color { get; set; }
}
