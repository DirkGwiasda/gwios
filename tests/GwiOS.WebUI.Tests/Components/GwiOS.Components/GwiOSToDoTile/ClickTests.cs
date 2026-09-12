using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;
using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using GwiOSToDoTileUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSToDoTile;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSToDoTile;

/// <summary>
/// Covers the buttons of <c>GwiOSToDoTile</c>.
/// </summary>
public sealed class ClickTests : BunitContext
{
    private int _completeCount;
    private int _deleteCount;

    public ClickTests()
    {
        Services.AddSingleton<TimeProvider>(new TimeProviderFake());
    }

    [Fact]
    public void RaisesOnComplete_WhenTheCompleteButtonIsClicked()
    {
        IRenderedComponent<GwiOSToDoTileUnderTest> tile = RenderTile(new ToDo { Title = "Einkaufen" });

        tile.Find(".gwios-icon-button--success").Click();

        Assert.Equal(1, _completeCount);
        Assert.Equal(0, _deleteCount);
    }

    [Fact]
    public void RaisesOnDelete_WhenTheDeleteButtonIsClicked()
    {
        IRenderedComponent<GwiOSToDoTileUnderTest> tile = RenderTile(new ToDo { Title = "Einkaufen" });

        tile.Find("button[title='Löschen']").Click();

        Assert.Equal(1, _deleteCount);
        Assert.Equal(0, _completeCount);
    }

    private IRenderedComponent<GwiOSToDoTileUnderTest> RenderTile(ToDo toDo)
        => Render<GwiOSToDoTileUnderTest>(parameters => parameters
            .Add(component => component.ToDo, toDo)
            .Add(component => component.OnComplete, () => _completeCount++)
            .Add(component => component.OnDelete, () => _deleteCount++));
}
