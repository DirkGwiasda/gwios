using AngleSharp.Dom;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using LogViewerUnderTest = GwiOS.WebUI.Components.Pages.Admin.LogViewer;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.LogViewer;

/// <summary>
/// Covers how <c>LogViewer</c> shows the stored log entries.
/// </summary>
public sealed class RenderingTests : LogViewerProbe
{
    [Fact]
    public void OffersOnlyTheChoiceOfAllApplicationsAndShowsAnEmptyText_WhenNoEntriesAreStored()
    {
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        Assert.Equal(["Alle"], GetAppChoices(page));
        Assert.Equal("Keine Log-Einträge für diese Auswahl.", page.Find(".gwios-empty").TextContent);
        Assert.Empty(GetShownMessages(page));
    }

    [Fact]
    public void OffersAllApplicationsFirstAndSelectsIt()
    {
        AddEntry("GwiOS", "Gestartet", LogLevel.Information, Morning);
        AddEntry("Backup", "Gesichert", LogLevel.Information, Morning.AddMinutes(1));

        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        IReadOnlyList<IElement> pills = page.FindAll(".gwios-pill");
        Assert.Equal(["Alle", "Backup", "GwiOS"], GetAppChoices(page));
        Assert.Equal("true", pills[0].GetAttribute("aria-pressed"));
        Assert.Equal(["Gesichert", "Gestartet"], GetShownMessages(page));
    }

    [Fact]
    public void ShowsTheEntriesNewestFirstWithLocalTimestampAndLevel()
    {
        AddEntry("GwiOS", "Älter", LogLevel.Warning, Morning);
        AddEntry("GwiOS", "Neuer", LogLevel.Error, Morning.AddMinutes(1));

        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        Assert.Equal(["Neuer", "Älter"], GetShownMessages(page));
        Assert.Equal(
            ["14.07.2026 10:15:05", "14.07.2026 10:14:05"],
            page.FindAll("td[data-label='Zeitstempel']").Select(cell => cell.TextContent.Trim()));
        Assert.Equal(["Error", "Warning"], page.FindAll(".gwios-log-level").Select(badge => badge.TextContent));
    }

    [Fact]
    public void ShowsOnlyTheFirstLineOfAMessage()
    {
        AddEntry("GwiOS", "Synchronisation fehlgeschlagen.\r\nDer Dienst antwortete mit HTTP 503.", LogLevel.Error, Morning);

        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        Assert.Equal(["Synchronisation fehlgeschlagen."], GetShownMessages(page));
    }

    [Fact]
    public void ShortensAFirstLineLongerThanNinetyCharacters()
    {
        string firstLine = new('x', 91);
        AddEntry("GwiOS", firstLine, LogLevel.Information, Morning);

        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        Assert.Equal([$"{new string('x', 90)} …"], GetShownMessages(page));
    }

    [Fact]
    public void ShowsTheFirstTenEntriesAndWhichEntriesThePageHolds()
    {
        AddEntries("GwiOS", "Eintrag", 12);

        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        Assert.Equal(10, GetShownMessages(page).Count);
        Assert.Equal("Eintrag 12", GetShownMessages(page)[0]);
        Assert.Equal("1–10 von 12", page.Find(".gwios-pager-range").TextContent);
    }

    [Fact]
    public void OffersToDeleteEntriesOnlyWhileASingleApplicationIsSelected()
    {
        AddEntry("GwiOS", "Gestartet", LogLevel.Information, Morning);
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();
        int buttonCountForAllApplications = page.FindAll(".gwios-log-actions button").Count;

        SelectApp(page, "GwiOS");

        Assert.Equal(1, buttonCountForAllApplications);
        Assert.Equal(2, page.FindAll(".gwios-log-actions button").Count);
    }

    [Fact]
    public void ShowsNoDialog_BeforeAnEntryIsOpened()
    {
        AddEntry("GwiOS", "Gestartet", LogLevel.Information, Morning);

        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        Assert.Empty(page.FindAll("[role='dialog']"));
    }
}
