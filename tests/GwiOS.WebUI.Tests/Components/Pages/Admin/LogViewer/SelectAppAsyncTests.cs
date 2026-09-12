using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using LogViewerUnderTest = GwiOS.WebUI.Components.Pages.Admin.LogViewer;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.LogViewer;

/// <summary>
/// Covers switching the application on <c>LogViewer</c>.
/// </summary>
public sealed class SelectAppAsyncTests : LogViewerProbe
{
    [Fact]
    public void ShowsTheEntriesOfTheChosenApplication()
    {
        DateTimeOffset timestamp = new(2026, 7, 14, 8, 0, 0, TimeSpan.Zero);
        AddEntry("Backup", "Gesichert", LogLevel.Information, timestamp);
        AddEntry("GwiOS", "Gestartet", LogLevel.Information, timestamp);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        page.FindAll(".gwios-pill")[1].Click();

        Assert.Equal(["Gestartet"], GetShownMessages(page));
    }
}
