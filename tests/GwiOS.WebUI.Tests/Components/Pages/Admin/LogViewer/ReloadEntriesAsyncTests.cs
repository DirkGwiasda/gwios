using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using LogViewerUnderTest = GwiOS.WebUI.Components.Pages.Admin.LogViewer;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.LogViewer;

/// <summary>
/// Covers refreshing the entries on <c>LogViewer</c>.
/// </summary>
public sealed class ReloadEntriesAsyncTests : LogViewerProbe
{
    [Fact]
    public void ShowsEntriesWrittenAfterThePageWasOpened()
    {
        DateTimeOffset timestamp = new(2026, 7, 14, 8, 0, 0, TimeSpan.Zero);
        AddEntry("GwiOS", "Gestartet", LogLevel.Information, timestamp);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();
        AddEntry("GwiOS", "ToDo angelegt", LogLevel.Information, timestamp.AddMinutes(5));

        page.Find(".gwios-log-actions button.gwios-button").Click();

        Assert.Equal(["ToDo angelegt", "Gestartet"], GetShownMessages(page));
    }
}
