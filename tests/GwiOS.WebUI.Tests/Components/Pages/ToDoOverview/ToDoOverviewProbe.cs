using GwiOS.Core.ToDos.Domain.Managers.ToDoManagement.Contracts;
using GwiOS.WebUI.StatusMessages.Contracts;
using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using ToDoOverviewUnderTest = GwiOS.WebUI.Components.Pages.ToDoOverview;

namespace GwiOS.WebUI.Tests.Components.Pages.ToDoOverview;

/// <summary>
/// Base of the <c>ToDoOverview</c> tests: provides an in-memory ToDo manager and status message service and a clock
/// standing at 2026-07-14.
/// </summary>
public abstract class ToDoOverviewProbe : BunitContext
{
    protected ToDoOverviewProbe()
    {
        Services.AddSingleton<IToDoManager>(ToDoManager);
        Services.AddSingleton<IStatusMessageService>(StatusMessages);
        Services.AddSingleton<TimeProvider>(new TimeProviderFake());
    }

    /// <summary>
    /// The ToDo manager the page works with.
    /// </summary>
    protected ToDoManagerFake ToDoManager { get; } = new();

    /// <summary>
    /// The status message service the page publishes to.
    /// </summary>
    protected StatusMessageServiceFake StatusMessages { get; } = new();

    /// <summary>
    /// Renders the page under test.
    /// </summary>
    protected IRenderedComponent<ToDoOverviewUnderTest> RenderPage()
        => Render<ToDoOverviewUnderTest>();

    /// <summary>
    /// Returns the titles of the ToDos shown in the section with the given title, in display order.
    /// </summary>
    protected static List<string> GetTitlesInSection(IRenderedComponent<ToDoOverviewUnderTest> page, string sectionTitle)
        => page.FindAll("section")
            .Single(section => section.QuerySelector("h2")?.TextContent == sectionTitle)
            .QuerySelectorAll(".gwios-todo-tile-title")
            .Select(title => title.TextContent)
            .ToList();
}
