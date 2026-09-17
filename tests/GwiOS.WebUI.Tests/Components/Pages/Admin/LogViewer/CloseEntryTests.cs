using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using LogViewerUnderTest = GwiOS.WebUI.Components.Pages.Admin.LogViewer;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.LogViewer;

/// <summary>
/// Covers closing the details of an entry on <c>LogViewer</c>.
/// </summary>
public sealed class CloseEntryTests : LogViewerProbe
{
    [Fact]
    public void RemovesTheDialog_WhenTheCloseButtonIsClicked()
    {
        AddEntry("GwiOS", "Gestartet", LogLevel.Information, Morning);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();
        page.Find("tr.gwios-log-row").Click();

        page.Find(".gwios-dialog-close").Click();

        Assert.Empty(page.FindAll("[role='dialog']"));
    }
}
