using AngleSharp.Dom;
using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts.Models;
using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using GwiOSToDoTileUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSToDoTile;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSToDoTile;

/// <summary>
/// Covers how <c>GwiOSToDoTile</c> renders open and completed ToDos. The clock stands at 2026-07-14.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    private readonly TimeProviderFake _timeProvider = new();

    public RenderingTests()
    {
        Services.AddSingleton<TimeProvider>(_timeProvider);
    }

    [Fact]
    public void ShowsTheTitleWithCompleteAndDeleteButtons_WhenTheToDoIsOpen()
    {
        IRenderedComponent<GwiOSToDoTileUnderTest> tile = RenderTile(new ToDo { Title = "Einkaufen" });

        Assert.Equal("Einkaufen", tile.Find(".gwios-todo-tile-title").TextContent);
        Assert.Equal(
            ["„Einkaufen“ als erledigt markieren", "„Einkaufen“ löschen"],
            tile.FindAll("button").Select(button => button.GetAttribute("aria-label")));
    }

    [Fact]
    public void ShowsOnlyTheDeleteButtonAndStrikesThrough_WhenTheToDoIsCompleted()
    {
        IRenderedComponent<GwiOSToDoTileUnderTest> tile = RenderTile(new ToDo { Title = "Einkaufen", IsCompleted = true });

        Assert.Equal("„Einkaufen“ löschen", Assert.Single(tile.FindAll("button")).GetAttribute("aria-label"));
        Assert.True(tile.Find("article").ClassList.Contains("gwios-todo-tile--completed"));
    }

    [Fact]
    public void ShowsTheDueDateWithoutYear_WhenItIsInTheCurrentYear()
    {
        IRenderedComponent<GwiOSToDoTileUnderTest> tile =
            RenderTile(new ToDo { Title = "Zahnarzt", DueDate = new DateOnly(2026, 8, 5) });

        Assert.Equal("⏰ 05.08.", tile.Find(".gwios-due-date").TextContent.Trim());
    }

    [Fact]
    public void ShowsTheDueDateWithYear_WhenItIsInAnotherYear()
    {
        IRenderedComponent<GwiOSToDoTileUnderTest> tile =
            RenderTile(new ToDo { Title = "Zahnarzt", DueDate = new DateOnly(2027, 1, 12) });

        Assert.Equal("⏰ 12.01.2027", tile.Find(".gwios-due-date").TextContent.Trim());
    }

    [Fact]
    public void HighlightsTheDueDate_WhenItHasPassed()
    {
        IRenderedComponent<GwiOSToDoTileUnderTest> tile =
            RenderTile(new ToDo { Title = "Steuern", DueDate = new DateOnly(2026, 7, 13) });

        IElement dueDate = tile.Find(".gwios-due-date");
        Assert.True(dueDate.ClassList.Contains("gwios-due-date--overdue"));
        Assert.Equal("Überfällig", dueDate.GetAttribute("title"));
    }

    [Fact]
    public void DoesNotHighlightTheDueDate_WhenItIsToday()
    {
        IRenderedComponent<GwiOSToDoTileUnderTest> tile =
            RenderTile(new ToDo { Title = "Steuern", DueDate = new DateOnly(2026, 7, 14) });

        Assert.False(tile.Find(".gwios-due-date").ClassList.Contains("gwios-due-date--overdue"));
    }

    [Fact]
    public void DeterminesTodayInTheLocalTimeZone()
    {
        // 2026-07-14 23:30 UTC is already 2026-07-15 in a zone two hours ahead.
        _timeProvider.UtcNow = new DateTimeOffset(2026, 7, 14, 23, 30, 0, TimeSpan.Zero);
        _timeProvider.UseLocalOffset(TimeSpan.FromHours(2));

        IRenderedComponent<GwiOSToDoTileUnderTest> tile =
            RenderTile(new ToDo { Title = "Steuern", DueDate = new DateOnly(2026, 7, 14) });

        Assert.True(tile.Find(".gwios-due-date").ClassList.Contains("gwios-due-date--overdue"));
    }

    [Fact]
    public void ShowsNoDueDate_WhenTheToDoHasNone()
    {
        IRenderedComponent<GwiOSToDoTileUnderTest> tile = RenderTile(new ToDo { Title = "Einkaufen" });

        Assert.Empty(tile.FindAll(".gwios-due-date"));
    }

    [Fact]
    public void ShowsNoDueDate_WhenTheToDoIsCompleted()
    {
        IRenderedComponent<GwiOSToDoTileUnderTest> tile = RenderTile(
            new ToDo { Title = "Steuern", DueDate = new DateOnly(2026, 7, 1), IsCompleted = true });

        Assert.Empty(tile.FindAll(".gwios-due-date"));
    }

    private IRenderedComponent<GwiOSToDoTileUnderTest> RenderTile(ToDo toDo)
        => Render<GwiOSToDoTileUnderTest>(parameters => parameters.Add(component => component.ToDo, toDo));
}
