using GwiOS.Core.CrossCutting.Logging.Domain.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts;
using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using LogViewerUnderTest = GwiOS.WebUI.Components.Pages.Admin.LogViewer;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.LogViewer;

/// <summary>
/// Base of the <c>LogViewer</c> tests: provides an in-memory log entry manager and status message service and a clock
/// whose local time zone is two hours ahead of UTC.
/// </summary>
public abstract class LogViewerProbe : BunitContext
{
    protected LogViewerProbe()
    {
        TimeProvider timeProvider = CreateTimeProvider();
        Services.AddSingleton<ILogEntryManager>(LogEntryManager);
        Services.AddSingleton<IStatusMessageService>(StatusMessages);
        Services.AddSingleton(timeProvider);
    }

    /// <summary>
    /// The log entry manager the page works with.
    /// </summary>
    protected LogEntryManagerFake LogEntryManager { get; } = new();

    /// <summary>
    /// The status message service the page publishes to.
    /// </summary>
    protected StatusMessageServiceFake StatusMessages { get; } = new();

    /// <summary>
    /// Stores an entry of the given application with the given message, level and UTC time.
    /// </summary>
    protected void AddEntry(string appName, string message, LogLevel level, DateTimeOffset timestamp)
        => LogEntryManager.Add(new LogEntry(appName, "Test", message, level) { Timestamp = timestamp });

    /// <summary>
    /// Renders the page under test.
    /// </summary>
    protected IRenderedComponent<LogViewerUnderTest> RenderPage()
        => Render<LogViewerUnderTest>();

    /// <summary>
    /// Returns the messages shown in the table, in display order.
    /// </summary>
    protected static List<string> GetShownMessages(IRenderedComponent<LogViewerUnderTest> page)
        => [.. page.FindAll("td[data-label='Nachricht']").Select(cell => cell.TextContent)];

    private static TimeProviderFake CreateTimeProvider()
    {
        TimeProviderFake timeProvider = new();
        timeProvider.UseLocalOffset(TimeSpan.FromHours(2));
        return timeProvider;
    }
}
