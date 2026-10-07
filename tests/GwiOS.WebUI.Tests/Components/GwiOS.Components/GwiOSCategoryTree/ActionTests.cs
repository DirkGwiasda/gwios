using AngleSharp.Dom;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOSCategoryTreeUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSCategoryTree;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSCategoryTree;

/// <summary>
/// Covers how <c>GwiOSCategoryTree</c> reports the actions the user takes on a category.
/// </summary>
public sealed class ActionTests : BunitContext
{
    private readonly GwiOSCategoryTreeProbe _probe = new();
    private readonly List<string> _reports = [];

    [Fact]
    public void ReportsTheCategoryToCollapse_WhenItsToggleIsClicked()
    {
        IRenderedComponent<GwiOSCategoryTreeUnderTest> tree = RenderCategoryTree();

        GetRow(tree, "Wohnen").QuerySelector(".gwios-category-toggle")!.Click();

        Assert.Equal([$"toggle {_probe.Living.Id}"], _reports);
    }

    [Fact]
    public void ReportsTheParentOfANewSubCategory_WhenItsAddButtonIsClicked()
    {
        IRenderedComponent<GwiOSCategoryTreeUnderTest> tree = RenderCategoryTree();

        GetRow(tree, "Miete").QuerySelector("button[title='Unterkategorie anlegen']")!.Click();

        Assert.Equal([$"add {_probe.Rent.Id}"], _reports);
    }

    [Fact]
    public void ReportsTheCategoryToEdit_WhenItsEditButtonIsClicked()
    {
        IRenderedComponent<GwiOSCategoryTreeUnderTest> tree = RenderCategoryTree();

        GetRow(tree, "Strom").QuerySelector("button[title='Bearbeiten']")!.Click();

        Assert.Equal(["edit Strom"], _reports);
    }

    [Fact]
    public void AsksForConfirmationBeforeReportingTheCategoryToDelete()
    {
        IRenderedComponent<GwiOSCategoryTreeUnderTest> tree = RenderCategoryTree();

        GetRow(tree, "Haushalt").QuerySelector("button[title='Löschen']")!.Click();

        IElement confirmButton = GetRow(tree, "Haushalt").QuerySelector(".gwios-category-actions button:last-child")!;
        Assert.Equal("Sicher?", confirmButton.TextContent.Trim());
        Assert.Empty(_reports);

        confirmButton.Click();

        Assert.Equal(["delete Haushalt"], _reports);
    }

    private IRenderedComponent<GwiOSCategoryTreeUnderTest> RenderCategoryTree()
        => Render<GwiOSCategoryTreeUnderTest>(parameters => parameters
            .Add(component => component.Categories, _probe.All)
            .Add(component => component.Sums, new Dictionary<Guid, decimal>())
            .Add(component => component.CollapsedCategoryIds, new HashSet<Guid>())
            .Add(component => component.OnToggleCollapse, (Guid id) => _reports.Add($"toggle {id}"))
            .Add(component => component.OnAddChild, (Guid? id) => _reports.Add($"add {id}"))
            .Add(component => component.OnEdit, (Category category) => _reports.Add($"edit {category.Name}"))
            .Add(component => component.OnDelete, (Category category) => _reports.Add($"delete {category.Name}")));

    private static IElement GetRow(IRenderedComponent<GwiOSCategoryTreeUnderTest> tree, string name)
        => tree.FindAll(".gwios-category-row").Single(row => row.QuerySelector(".gwios-category-name")!.TextContent == name);
}
