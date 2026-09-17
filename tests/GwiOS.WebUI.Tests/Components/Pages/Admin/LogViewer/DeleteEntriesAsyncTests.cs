using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using LogViewerUnderTest = GwiOS.WebUI.Components.Pages.Admin.LogViewer;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.LogViewer;

/// <summary>
/// Covers deleting the entries of an application on <c>LogViewer</c>.
/// </summary>
public sealed class DeleteEntriesAsyncTests : LogViewerProbe
{
    [Fact]
    public void DeletesNothing_OnTheFirstClick()
    {
        AddEntry("Backup", "Gesichert", LogLevel.Information, Morning);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();
        SelectApp(page, "Backup");

        ClickDeleteButton(page);

        Assert.Equal(1, LogEntryManager.CountEntries("Backup"));
        Assert.Equal("Wirklich alle löschen?", page.Find(".gwios-button--danger").TextContent.Trim());
    }

    [Fact]
    public void DeletesOnlyTheEntriesOfTheSelectedApplicationAndShowsAllApplications_WhenConfirmed()
    {
        AddEntry("Backup", "Gesichert", LogLevel.Information, Morning);
        AddEntry("GwiOS", "Gestartet", LogLevel.Information, Morning);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();
        SelectApp(page, "Backup");

        ClickDeleteButton(page);
        ClickDeleteButton(page);

        Assert.Equal(0, LogEntryManager.CountEntries("Backup"));
        Assert.Equal(1, LogEntryManager.CountEntries("GwiOS"));
        Assert.Equal(["Alle", "GwiOS"], GetAppChoices(page));
        Assert.Equal("true", page.Find(".gwios-pill").GetAttribute("aria-pressed"));
        Assert.Equal(["Gestartet"], GetShownMessages(page));
    }

    [Fact]
    public void ShowsTheEmptyText_WhenTheLastApplicationWasDeleted()
    {
        AddEntry("Backup", "Gesichert", LogLevel.Information, Morning);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();
        SelectApp(page, "Backup");

        ClickDeleteButton(page);
        ClickDeleteButton(page);

        Assert.Equal(["Alle"], GetAppChoices(page));
        Assert.Equal("Keine Log-Einträge für diese Auswahl.", page.Find(".gwios-empty").TextContent);
    }

    [Fact]
    public void PublishesANeutralStatusMessage()
    {
        AddEntry("Backup", "Gesichert", LogLevel.Information, Morning);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();
        SelectApp(page, "Backup");

        ClickDeleteButton(page);
        ClickDeleteButton(page);

        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Neutral, message.Level);
        Assert.Equal("Log-Einträge von „Backup“ gelöscht", message.Text);
    }

    private static void ClickDeleteButton(IRenderedComponent<LogViewerUnderTest> page)
        => page.FindAll(".gwios-log-actions button")[1].Click();
}
