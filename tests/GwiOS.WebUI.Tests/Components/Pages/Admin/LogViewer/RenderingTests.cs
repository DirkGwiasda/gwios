using AngleSharp.Dom;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using LogViewerUnderTest = GwiOS.WebUI.Components.Pages.Admin.LogViewer;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.LogViewer;

/// <summary>
/// Covers how <c>LogViewer</c> shows the stored log entries.
/// </summary>
public sealed class RenderingTests : LogViewerProbe
{
    private readonly DateTimeOffset _morning = new(2026, 7, 14, 8, 14, 5, TimeSpan.Zero);

    [Fact]
    public void ShowsAnEmptyText_WhenNoEntriesAreStored()
    {
        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        Assert.Equal("Keine Log-Einträge vorhanden.", page.Find(".gwios-empty").TextContent);
        Assert.Empty(page.FindAll("table"));
    }

    [Fact]
    public void OffersTheApplicationsAndSelectsTheFirst()
    {
        AddEntry("GwiOS", "Gestartet", LogLevel.Information, _morning);
        AddEntry("Backup", "Gesichert", LogLevel.Information, _morning);

        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        IReadOnlyList<IElement> pills = page.FindAll(".gwios-pill");
        Assert.Equal(["Backup", "GwiOS"], pills.Select(pill => pill.TextContent.Trim()));
        Assert.Equal("true", pills[0].GetAttribute("aria-pressed"));
        Assert.Equal(["Gesichert"], GetShownMessages(page));
    }

    [Fact]
    public void ShowsTheEntriesNewestFirstWithLocalTimestampAndLevel()
    {
        AddEntry("GwiOS", "Älter", LogLevel.Warning, _morning);
        AddEntry("GwiOS", "Neuer", LogLevel.Error, _morning.AddMinutes(1));

        IRenderedComponent<LogViewerUnderTest> page = RenderPage();

        Assert.Equal(["Neuer", "Älter"], GetShownMessages(page));
        Assert.Equal(
            ["14.07.2026 10:15:05", "14.07.2026 10:14:05"],
            page.FindAll("td[data-label='Zeitstempel']").Select(cell => cell.TextContent));
        Assert.Equal(["ERROR", "WARN"], page.FindAll(".gwios-log-level").Select(badge => badge.TextContent));
    }
}
