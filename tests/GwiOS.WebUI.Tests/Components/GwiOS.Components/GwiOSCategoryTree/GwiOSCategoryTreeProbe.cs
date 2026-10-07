using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSCategoryTree;

/// <summary>
/// A small category tree for the tests of <c>GwiOSCategoryTree</c>: two root categories, one of them with two
/// sub-categories.
/// </summary>
internal sealed class GwiOSCategoryTreeProbe
{
    public Category Living { get; } = new() { Name = "Wohnen", Type = CategoryType.Expense, Color = "#ff4d6d" };

    public Category Household { get; } = new() { Name = "Haushalt", Type = CategoryType.Expense, Color = "#ffd056" };

    public Category Rent { get; }

    public Category Electricity { get; }

    public GwiOSCategoryTreeProbe()
    {
        Rent = new Category { Name = "Miete", Type = CategoryType.Expense, ParentId = Living.Id };
        Electricity = new Category { Name = "Strom", Type = CategoryType.Expense, ParentId = Living.Id };
    }

    public IReadOnlyList<Category> All
        => [Living, Household, Rent, Electricity];
}
