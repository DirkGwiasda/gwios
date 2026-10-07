using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOSCategoryLabelUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSCategoryLabel;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSCategoryLabel;

/// <summary>
/// Covers how <c>GwiOSCategoryLabel</c> shows the path and color of a category.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    private readonly Category _household = new() { Name = "Haushalt", Type = CategoryType.Expense, Color = "#ffd056" };
    private readonly Category _groceries;
    private readonly Category _vegetables;

    public RenderingTests()
    {
        _groceries = new Category { Name = "Lebensmittel", Type = CategoryType.Expense, ParentId = _household.Id };
        _vegetables = new Category { Name = "Gemüse", Type = CategoryType.Expense, ParentId = _groceries.Id };
    }

    [Fact]
    public void ShowsThePathFromTheRootCategoryDown()
    {
        IRenderedComponent<GwiOSCategoryLabelUnderTest> label = RenderLabel(_vegetables.Id);

        Assert.Equal("Haushalt › Lebensmittel › Gemüse", label.Find(".gwios-category-label-path").TextContent);
    }

    [Fact]
    public void ShowsTheColorOfTheRootCategory()
    {
        IRenderedComponent<GwiOSCategoryLabelUnderTest> label = RenderLabel(_vegetables.Id);

        Assert.Contains("#ffd056", label.Find(".gwios-category-label-dot").GetAttribute("style"));
    }

    [Fact]
    public void ShowsADashAndAGrayDot_WhenTheCategoryIsUnknown()
    {
        IRenderedComponent<GwiOSCategoryLabelUnderTest> label = RenderLabel(Guid.NewGuid());

        Assert.Equal("—", label.Find(".gwios-category-label-path").TextContent);
        Assert.Contains("#7c93b3", label.Find(".gwios-category-label-dot").GetAttribute("style"));
    }

    private IRenderedComponent<GwiOSCategoryLabelUnderTest> RenderLabel(Guid categoryId)
        => Render<GwiOSCategoryLabelUnderTest>(parameters => parameters
            .Add(component => component.Categories, [_household, _groceries, _vegetables])
            .Add(component => component.CategoryId, categoryId));
}
