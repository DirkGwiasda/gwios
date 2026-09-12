using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;
using ToDoOverviewUnderTest = GwiOS.WebUI.Components.Pages.ToDoOverview;

namespace GwiOS.WebUI.Tests.Components.Pages.ToDoOverview;

/// <summary>
/// Covers how <c>ToDoOverview</c> shows the stored ToDos.
/// </summary>
public sealed class RenderingTests : ToDoOverviewProbe
{
    [Fact]
    public void ShowsOpenAndCompletedToDosInSeparateSectionsWithTheirCounts()
    {
        ToDoManager.Add(new ToDo { Title = "Einkaufen" });
        ToDoManager.Add(new ToDo { Title = "Steuern", IsCompleted = true });
        ToDoManager.Add(new ToDo { Title = "Zahnarzt" });

        IRenderedComponent<ToDoOverviewUnderTest> page = RenderPage();

        Assert.Equal(["Einkaufen", "Zahnarzt"], GetTitlesInSection(page, "Offen"));
        Assert.Equal(["Steuern"], GetTitlesInSection(page, "Erledigt"));
        Assert.Equal(["2", "1"], page.FindAll(".gwios-section-count").Select(count => count.TextContent));
    }

    [Fact]
    public void ShowsEmptyTexts_WhenNoToDosAreStored()
    {
        IRenderedComponent<ToDoOverviewUnderTest> page = RenderPage();

        Assert.Equal(
            ["Keine offenen Aufgaben.", "Keine erledigten Aufgaben."],
            page.FindAll(".gwios-empty").Select(text => text.TextContent));
    }

    [Fact]
    public void DisablesTheAddButton_WhileNoTitleIsEntered()
    {
        IRenderedComponent<ToDoOverviewUnderTest> page = RenderPage();

        Assert.True(page.Find(".gwios-toolbar button").HasAttribute("disabled"));
    }

    [Fact]
    public void EnablesTheAddButton_WhenATitleIsEntered()
    {
        IRenderedComponent<ToDoOverviewUnderTest> page = RenderPage();

        page.Find(".gwios-toolbar input[type='text']").Input("Einkaufen");

        Assert.False(page.Find(".gwios-toolbar button").HasAttribute("disabled"));
    }
}
