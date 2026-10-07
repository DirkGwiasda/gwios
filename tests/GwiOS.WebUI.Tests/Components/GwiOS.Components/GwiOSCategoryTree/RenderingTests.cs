using System.Globalization;
using AngleSharp.Dom;
using GwiOSCategoryTreeUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSCategoryTree;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSCategoryTree;

/// <summary>
/// Covers how <c>GwiOSCategoryTree</c> renders the levels, sums and colors of the categories.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    private readonly GwiOSCategoryTreeProbe _probe = new();

    [Fact]
    public void RendersEachRootCategoryFollowedByItsSubCategories_SortedByName()
    {
        IRenderedComponent<GwiOSCategoryTreeUnderTest> tree = RenderCategoryTree(collapsedCategoryIds: []);

        Assert.Equal(["Haushalt", "Wohnen", "Miete", "Strom"], GetNames(tree));
    }

    [Fact]
    public void MarksRootAndSubCategoriesDifferently()
    {
        IRenderedComponent<GwiOSCategoryTreeUnderTest> tree = RenderCategoryTree(collapsedCategoryIds: []);

        IReadOnlyList<IElement> rows = tree.FindAll(".gwios-category-row");
        Assert.True(rows[1].ClassList.Contains("gwios-category-row--root"));
        Assert.True(rows[2].ClassList.Contains("gwios-category-row--child"));
    }

    [Fact]
    public void IndentsSubCategories()
    {
        IRenderedComponent<GwiOSCategoryTreeUnderTest> tree = RenderCategoryTree(collapsedCategoryIds: []);

        IReadOnlyList<IElement> rows = tree.FindAll(".gwios-category-row");
        Assert.Equal("padding-left: 14px;", rows[1].GetAttribute("style"));
        Assert.Equal("padding-left: 34px;", rows[2].GetAttribute("style"));
    }

    [Fact]
    public void HidesTheSubCategoriesOfACollapsedCategory()
    {
        IRenderedComponent<GwiOSCategoryTreeUnderTest> tree = RenderCategoryTree(collapsedCategoryIds: [_probe.Living.Id]);

        Assert.Equal(["Haushalt", "Wohnen"], GetNames(tree));
    }

    [Fact]
    public void ShowsTheSumInEuros_OrADash_WhenThereIsNone()
    {
        IRenderedComponent<GwiOSCategoryTreeUnderTest> tree = RenderCategoryTree(collapsedCategoryIds: []);

        Assert.Equal(["—", FormatEuros(1248m), FormatEuros(1150m), FormatEuros(98m)], tree.FindAll(".gwios-category-sum").Select(sum => sum.TextContent));
    }

    [Fact]
    public void ShowsSubCategoriesInTheColorOfTheirRootCategory()
    {
        IRenderedComponent<GwiOSCategoryTreeUnderTest> tree = RenderCategoryTree(collapsedCategoryIds: []);

        Assert.All(tree.FindAll(".gwios-category-dot").Skip(1), dot => Assert.Contains("#ff4d6d", dot.GetAttribute("style")));
    }

    [Fact]
    public void DisablesTheToggle_WhenTheCategoryHasNoSubCategories()
    {
        IRenderedComponent<GwiOSCategoryTreeUnderTest> tree = RenderCategoryTree(collapsedCategoryIds: []);

        IReadOnlyList<IElement> toggles = tree.FindAll(".gwios-category-toggle");
        Assert.True(toggles[0].HasAttribute("disabled"));
        Assert.False(toggles[1].HasAttribute("disabled"));
    }

    private IRenderedComponent<GwiOSCategoryTreeUnderTest> RenderCategoryTree(HashSet<Guid> collapsedCategoryIds)
        => Render<GwiOSCategoryTreeUnderTest>(parameters => parameters
            .Add(component => component.Categories, _probe.All)
            .Add(component => component.Sums, new Dictionary<Guid, decimal>
            {
                [_probe.Living.Id] = 1248m,
                [_probe.Rent.Id] = 1150m,
                [_probe.Electricity.Id] = 98m
            })
            .Add(component => component.CollapsedCategoryIds, collapsedCategoryIds));

    private static string FormatEuros(decimal value)
        => value.ToString("C", CultureInfo.GetCultureInfo("de-DE"));

    private static IEnumerable<string> GetNames(IRenderedComponent<GwiOSCategoryTreeUnderTest> tree)
        => tree.FindAll(".gwios-category-name").Select(name => name.TextContent);
}
