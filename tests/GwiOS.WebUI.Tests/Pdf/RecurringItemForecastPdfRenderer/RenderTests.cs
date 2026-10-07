using System.Text;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts.Models;
using GwiOS.WebUI.Tests.TestInfrastructure;
using QuestPdfConfiguratorImplementation = GwiOS.WebUI.Pdf.QuestPdfConfigurator;
using RecurringItemForecastPdfRendererUnderTest = GwiOS.WebUI.Pdf.RecurringItemForecastPdfRenderer;

namespace GwiOS.WebUI.Tests.Pdf.RecurringItemForecastPdfRenderer;

/// <summary>
/// Covers <c>RecurringItemForecastPdfRenderer.Render</c> with the real QuestPDF library and the embedded fonts.
/// </summary>
public sealed class RenderTests
{
    private readonly LoggerFake<RecurringItemForecastPdfRendererUnderTest> _logger = new();
    private readonly RecurringItemForecastPdfRendererUnderTest _renderer;
    private readonly DateTimeOffset _createdAt = new(2026, 10, 7, 14, 30, 0, TimeSpan.FromHours(2));

    public RenderTests()
    {
        _renderer = new RecurringItemForecastPdfRendererUnderTest(
            new QuestPdfConfiguratorImplementation(new LoggerFake<QuestPdfConfiguratorImplementation>()),
            _logger);
    }

    [Fact]
    public void ReturnsAPdfDocument()
    {
        byte[] pdf = _renderer.Render(CreateForecast(new TestRecurringItemForecasts().CreateOctober()), _createdAt);

        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
    }

    [Fact]
    public void EmbedsTheInterFont()
    {
        byte[] pdf = _renderer.Render(CreateForecast(new TestRecurringItemForecasts().CreateOctober()), _createdAt);

        Assert.Contains("Inter-Regular", Encoding.Latin1.GetString(pdf));
    }

    [Fact]
    public void RendersMonthsWithoutDueItems()
    {
        RecurringItemForecastData forecast = CreateForecast(
            TestRecurringItemForecasts.CreateEmpty(new DateOnly(2026, 10, 1)),
            TestRecurringItemForecasts.CreateEmpty(new DateOnly(2026, 11, 1)));

        byte[] pdf = _renderer.Render(forecast, _createdAt);

        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
    }

    [Fact]
    public void RendersAForecastLongerThanOnePage()
    {
        TestRecurringItemForecasts forecasts = new();
        RecurringItemForecastData forecast = CreateForecast([.. Enumerable.Range(0, 12).Select(_ => forecasts.CreateOctober())]);

        byte[] pdf = _renderer.Render(forecast, _createdAt);

        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
    }

    [Fact]
    public void LogsTheRenderingWithMonthCountAndSize()
    {
        byte[] pdf = _renderer.Render(CreateForecast(new TestRecurringItemForecasts().CreateOctober()), _createdAt);

        LogEntry logEntry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Debug, logEntry.LogLevel);
        Assert.Equal("1", logEntry.ContextData["MonthCount"]);
        Assert.Equal(pdf.Length.ToString(), logEntry.ContextData["Bytes"]);
    }

    private static RecurringItemForecastData CreateForecast(params RecurringItemForecastMonth[] months)
        => new() { Months = months };
}
