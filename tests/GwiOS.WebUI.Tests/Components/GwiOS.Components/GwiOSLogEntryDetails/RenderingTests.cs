using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using GwiOSLogEntryDetailsUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSLogEntryDetails;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSLogEntryDetails;

/// <summary>
/// Covers how <c>GwiOSLogEntryDetails</c> shows all details of a log entry, using a clock whose local time zone is two
/// hours ahead of UTC.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    private readonly LogEntry _entry = new(
        "GwiOS.WebUI",
        "GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.PersonManager",
        "Synchronisation fehlgeschlagen.\nDer Dienst antwortete mit HTTP 503.",
        LogLevel.Error,
        new Dictionary<string, string> { ["RetryCount"] = "3", ["HttpStatus"] = "503", ["endpoint"] = "/users" })
    {
        Id = Guid.Parse("0192f3a4-8c1b-7d42-9f6e-3a7c51b2d904"),
        Timestamp = new DateTimeOffset(2026, 9, 12, 15, 42, 9, TimeSpan.Zero)
    };

    public RenderingTests()
    {
        TimeProviderFake timeProvider = new();
        timeProvider.UseLocalOffset(TimeSpan.FromHours(2));
        Services.AddSingleton<TimeProvider>(timeProvider);
    }

    [Fact]
    public void ShowsTheLocalTimestampTheApplicationAndTheSource()
    {
        IRenderedComponent<GwiOSLogEntryDetailsUnderTest> details = RenderDetails(_entry);

        Assert.Equal(
            ["Zeitstempel", "Anwendung", "Quelle"],
            details.FindAll(".gwios-log-details-facts dt").Select(term => term.TextContent));
        Assert.Equal("12.09.2026 17:42:09", details.Find(".gwios-log-details-timestamp").TextContent);
        Assert.Equal("GwiOS.WebUI", details.Find(".gwios-log-details-app").TextContent);
        Assert.Equal(_entry.DataSource, details.Find(".gwios-log-details-source").TextContent);
    }

    [Fact]
    public void ShowsTheWholeMessageWithAllLines()
    {
        IRenderedComponent<GwiOSLogEntryDetailsUnderTest> details = RenderDetails(_entry);

        Assert.Equal(_entry.Message, details.Find(".gwios-log-details-message").TextContent);
    }

    [Fact]
    public void ShowsTheContextDataOrderedByKey()
    {
        IRenderedComponent<GwiOSLogEntryDetailsUnderTest> details = RenderDetails(_entry);

        Assert.Equal(
            ["endpoint", "HttpStatus", "RetryCount"],
            details.FindAll(".gwios-log-details-context dt").Select(key => key.TextContent));
        Assert.Equal(
            ["/users", "503", "3"],
            details.FindAll(".gwios-log-details-context dd").Select(value => value.TextContent));
        Assert.Empty(details.FindAll(".gwios-log-details-context-empty"));
    }

    [Fact]
    public void SaysSo_WhenTheEntryHasNoContextData()
    {
        IRenderedComponent<GwiOSLogEntryDetailsUnderTest> details =
            RenderDetails(_entry with { ContextData = new Dictionary<string, string>() });

        Assert.Equal("Keine Kontextdaten", details.Find(".gwios-log-details-context-empty").TextContent);
        Assert.Empty(details.FindAll(".gwios-log-details-context"));
    }

    [Fact]
    public void ShowsTheIdOfTheEntry()
    {
        IRenderedComponent<GwiOSLogEntryDetailsUnderTest> details = RenderDetails(_entry);

        Assert.Equal("0192f3a4-8c1b-7d42-9f6e-3a7c51b2d904", details.Find(".gwios-log-details-id-value").TextContent);
    }

    private IRenderedComponent<GwiOSLogEntryDetailsUnderTest> RenderDetails(LogEntry entry)
        => Render<GwiOSLogEntryDetailsUnderTest>(parameters => parameters.Add(component => component.Entry, entry));
}
