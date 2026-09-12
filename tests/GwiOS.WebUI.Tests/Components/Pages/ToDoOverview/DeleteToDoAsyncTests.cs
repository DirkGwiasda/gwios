using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using ToDoOverviewUnderTest = GwiOS.WebUI.Components.Pages.ToDoOverview;

namespace GwiOS.WebUI.Tests.Components.Pages.ToDoOverview;

/// <summary>
/// Covers deleting a ToDo on <c>ToDoOverview</c>.
/// </summary>
public sealed class DeleteToDoAsyncTests : ToDoOverviewProbe
{
    [Fact]
    public void DeletesOnlyTheChosenToDo()
    {
        ToDo shopping = new() { Title = "Einkaufen" };
        ToDoManager.Add(shopping);
        ToDoManager.Add(new ToDo { Title = "Steuern", IsCompleted = true });
        IRenderedComponent<ToDoOverviewUnderTest> page = RenderPage();

        page.Find("button[aria-label='„Einkaufen“ löschen']").Click();

        Assert.Equal("Steuern", Assert.Single(ToDoManager.ToDos).Title);
        Assert.Empty(GetTitlesInSection(page, "Offen"));
        Assert.Equal(["Steuern"], GetTitlesInSection(page, "Erledigt"));
    }

    [Fact]
    public void PublishesANeutralStatusMessage()
    {
        ToDoManager.Add(new ToDo { Title = "Steuern", IsCompleted = true });
        IRenderedComponent<ToDoOverviewUnderTest> page = RenderPage();

        page.Find("button[aria-label='„Steuern“ löschen']").Click();

        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Neutral, message.Level);
        Assert.Equal("ToDo „Steuern“ gelöscht", message.Text);
    }
}
