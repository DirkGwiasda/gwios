using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using ToDoOverviewUnderTest = GwiOS.WebUI.Components.Pages.ToDoOverview;

namespace GwiOS.WebUI.Tests.Components.Pages.ToDoOverview;

/// <summary>
/// Covers completing a ToDo on <c>ToDoOverview</c>.
/// </summary>
public sealed class CompleteToDoAsyncTests : ToDoOverviewProbe
{
    private readonly ToDo _toDo = new() { Title = "Einkaufen" };

    [Fact]
    public void MovesTheToDoToTheCompletedSection()
    {
        ToDoManager.Add(_toDo);
        IRenderedComponent<ToDoOverviewUnderTest> page = RenderPage();

        page.Find(".gwios-icon-button--success").Click();

        Assert.True(_toDo.IsCompleted);
        Assert.Empty(GetTitlesInSection(page, "Offen"));
        Assert.Equal(["Einkaufen"], GetTitlesInSection(page, "Erledigt"));
    }

    [Fact]
    public void PublishesASuccessStatusMessage()
    {
        ToDoManager.Add(_toDo);
        IRenderedComponent<ToDoOverviewUnderTest> page = RenderPage();

        page.Find(".gwios-icon-button--success").Click();

        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Success, message.Level);
        Assert.Equal("ToDo „Einkaufen“ erledigt", message.Text);
    }

    [Fact]
    public void PublishesAWarningAndRemovesTheToDo_WhenItWasDeletedMeanwhile()
    {
        ToDoManager.Add(_toDo);
        IRenderedComponent<ToDoOverviewUnderTest> page = RenderPage();
        ToDoManager.Remove(_toDo.Id);

        page.Find(".gwios-icon-button--success").Click();

        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Warning, message.Level);
        Assert.Equal("ToDo „Einkaufen“ wurde inzwischen gelöscht", message.Text);
        Assert.Empty(page.FindAll(".gwios-todo-tile"));
    }
}
