using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using LogViewerUnderTest = GwiOS.WebUI.Components.Pages.Admin.LogViewer;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.LogViewer;

/// <summary>
/// Covers switching the application on <c>LogViewer</c>.
/// </summary>
public sealed class SelectAppAsyncTests : LogViewerProbe
{
    [Fact]
    public void ShowsOnlyTheEntriesOfTheChosenApplication()
    {
        AddEntry("Backup", "Gesichert", LogLevel.Information, Morning);
        AddEntry("GwiOS", "Gestartet", LogLevel.Information, Morning);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        SelectApp(page, "GwiOS");

        Assert.Equal(["Gestartet"], GetShownMessages(page));
    }

    [Fact]
    public void ShowsTheEntriesOfAllApplicationsAgain_WhenAllIsChosen()
    {
        AddEntry("Backup", "Gesichert", LogLevel.Information, Morning);
        AddEntry("GwiOS", "Gestartet", LogLevel.Information, Morning.AddMinutes(1));
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();
        SelectApp(page, "GwiOS");

        SelectApp(page, "Alle");

        Assert.Equal(["Gestartet", "Gesichert"], GetShownMessages(page));
    }

    [Fact]
    public void ShowsTheFirstPageOfTheChosenApplication()
    {
        AddEntries("GwiOS", "Eintrag", 12);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();
        ClickPagerButton(page, "2");

        SelectApp(page, "GwiOS");

        Assert.Equal("Eintrag 12", GetShownMessages(page)[0]);
    }
}
