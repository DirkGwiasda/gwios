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
    /// <summary>
    /// A point in time the tests write their entries at or after.
    /// </summary>
    protected static readonly DateTimeOffset Morning = new(2026, 7, 14, 8, 14, 5, TimeSpan.Zero);

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
    /// Stores the given number of entries of the given application, one minute apart from <see cref="Morning"/> on;
    /// their messages are the prefix followed by their number, counting from 1 for the oldest.
    /// </summary>
    protected void AddEntries(string appName, string messagePrefix, int count)
    {
        for (int number = 1; number <= count; number++)
        {
            AddEntry(appName, $"{messagePrefix} {number}", LogLevel.Information, Morning.AddMinutes(number));
        }
    }

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

    /// <summary>
    /// Returns the labels of the application choices, in display order.
    /// </summary>
    protected static List<string> GetAppChoices(IRenderedComponent<LogViewerUnderTest> page)
        => [.. page.FindAll(".gwios-pill").Select(pill => pill.TextContent.Trim())];

    /// <summary>
    /// Clicks the application choice with the given label.
    /// </summary>
    protected static void SelectApp(IRenderedComponent<LogViewerUnderTest> page, string appChoice)
        => page.FindAll(".gwios-pill").Single(pill => pill.TextContent.Trim() == appChoice).Click();

    /// <summary>
    /// Clicks the button of the pager with the given text, such as a page number or a page size.
    /// </summary>
    protected static void ClickPagerButton(IRenderedComponent<LogViewerUnderTest> page, string text)
        => page.FindAll(".gwios-pager button").First(button => button.TextContent.Trim() == text).Click();

    private static TimeProviderFake CreateTimeProvider()
    {
        TimeProviderFake timeProvider = new();
        timeProvider.UseLocalOffset(TimeSpan.FromHours(2));
        return timeProvider;
    }
}
