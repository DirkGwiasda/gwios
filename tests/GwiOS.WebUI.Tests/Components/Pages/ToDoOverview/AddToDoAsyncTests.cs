using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using Microsoft.AspNetCore.Components.Web;
using ToDoOverviewUnderTest = GwiOS.WebUI.Components.Pages.ToDoOverview;

namespace GwiOS.WebUI.Tests.Components.Pages.ToDoOverview;

/// <summary>
/// Covers adding a ToDo on <c>ToDoOverview</c>.
/// </summary>
public sealed class AddToDoAsyncTests : ToDoOverviewProbe
{
    [Fact]
    public void CreatesTheToDoWithTheTrimmedTitleAndTheDueDate()
    {
        IRenderedComponent<ToDoOverviewUnderTest> page = RenderPage();

        page.Find(".gwios-toolbar input[type='text']").Input("  Zahnarzt  ");
        page.Find(".gwios-toolbar input[type='date']").Change("2026-08-05");
        page.Find(".gwios-toolbar button").Click();

        ToDo toDo = Assert.Single(ToDoManager.ToDos);
        Assert.Equal("Zahnarzt", toDo.Title);
        Assert.Equal(new DateOnly(2026, 8, 5), toDo.DueDate);
    }

    [Fact]
    public void ShowsTheNewToDoAndClearsTheInputs()
    {
        IRenderedComponent<ToDoOverviewUnderTest> page = RenderPage();

        page.Find(".gwios-toolbar input[type='text']").Input("Zahnarzt");
        page.Find(".gwios-toolbar input[type='date']").Change("2026-08-05");
        page.Find(".gwios-toolbar button").Click();

        Assert.Equal(["Zahnarzt"], GetTitlesInSection(page, "Offen"));
        Assert.Equal(string.Empty, page.Find(".gwios-toolbar input[type='text']").GetAttribute("value"));
        Assert.Equal(string.Empty, page.Find(".gwios-toolbar input[type='date']").GetAttribute("value"));
    }

    [Fact]
    public void CreatesTheToDo_WhenEnterIsPressedInTheTitle()
    {
        IRenderedComponent<ToDoOverviewUnderTest> page = RenderPage();

        page.Find(".gwios-toolbar input[type='text']").Input("Einkaufen");
        page.Find(".gwios-toolbar input[type='text']").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.Equal("Einkaufen", Assert.Single(ToDoManager.ToDos).Title);
    }

    [Fact]
    public void CreatesNothing_WhenEnterIsPressedWithoutTitle()
    {
        IRenderedComponent<ToDoOverviewUnderTest> page = RenderPage();

        page.Find(".gwios-toolbar input[type='text']").Input("   ");
        page.Find(".gwios-toolbar input[type='text']").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        Assert.Empty(ToDoManager.ToDos);
        Assert.Empty(StatusMessages.Messages);
    }

    [Fact]
    public void PublishesANeutralStatusMessage()
    {
        IRenderedComponent<ToDoOverviewUnderTest> page = RenderPage();

        page.Find(".gwios-toolbar input[type='text']").Input("Einkaufen");
        page.Find(".gwios-toolbar button").Click();

        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Neutral, message.Level);
        Assert.Equal("ToDo „Einkaufen“ hinzugefügt", message.Text);
    }
}
