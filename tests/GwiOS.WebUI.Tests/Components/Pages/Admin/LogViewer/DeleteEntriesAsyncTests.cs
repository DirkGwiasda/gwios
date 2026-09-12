using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using LogViewerUnderTest = GwiOS.WebUI.Components.Pages.Admin.LogViewer;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.LogViewer;

/// <summary>
/// Covers deleting the entries of an application on <c>LogViewer</c>.
/// </summary>
public sealed class DeleteEntriesAsyncTests : LogViewerProbe
{
    private readonly DateTimeOffset _timestamp = new(2026, 7, 14, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void DeletesNothing_OnTheFirstClick()
    {
        AddEntry("Backup", "Gesichert", LogLevel.Information, _timestamp);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        ClickDeleteButton(page);

        Assert.Equal(1, LogEntryManager.CountEntries("Backup"));
        Assert.Equal("Wirklich alle löschen?", page.Find(".gwios-button--danger").TextContent.Trim());
    }

    [Fact]
    public void DeletesOnlyTheEntriesOfTheSelectedApplicationAndSelectsTheNextOne_WhenConfirmed()
    {
        AddEntry("Backup", "Gesichert", LogLevel.Information, _timestamp);
        AddEntry("GwiOS", "Gestartet", LogLevel.Information, _timestamp);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        ClickDeleteButton(page);
        ClickDeleteButton(page);

        Assert.Equal(0, LogEntryManager.CountEntries("Backup"));
        Assert.Equal(1, LogEntryManager.CountEntries("GwiOS"));
        Assert.Equal(["GwiOS"], page.FindAll(".gwios-pill").Select(pill => pill.TextContent.Trim()));
        Assert.Equal(["Gestartet"], GetShownMessages(page));
    }

    [Fact]
    public void ShowsTheEmptyText_WhenTheLastApplicationWasDeleted()
    {
        AddEntry("Backup", "Gesichert", LogLevel.Information, _timestamp);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        ClickDeleteButton(page);
        ClickDeleteButton(page);

        Assert.Equal("Keine Log-Einträge vorhanden.", page.Find(".gwios-empty").TextContent);
    }

    [Fact]
    public void PublishesANeutralStatusMessage()
    {
        AddEntry("Backup", "Gesichert", LogLevel.Information, _timestamp);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        ClickDeleteButton(page);
        ClickDeleteButton(page);

        StatusMessage message = Assert.Single(StatusMessages.Messages);
        Assert.Equal(StatusLevel.Neutral, message.Level);
        Assert.Equal("Log-Einträge von „Backup“ gelöscht", message.Text);
    }

    private static void ClickDeleteButton(IRenderedComponent<LogViewerUnderTest> page)
        => page.FindAll(".gwios-log-actions button")[1].Click();
}
