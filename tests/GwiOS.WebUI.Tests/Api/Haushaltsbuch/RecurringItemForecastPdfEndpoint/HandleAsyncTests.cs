using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts.Models;
using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using RecurringItemForecastPdfEndpointUnderTest = GwiOS.WebUI.Api.Haushaltsbuch.RecurringItemForecastPdfEndpoint;

namespace GwiOS.WebUI.Tests.Api.Haushaltsbuch.RecurringItemForecastPdfEndpoint;

/// <summary>
/// Covers <c>RecurringItemForecastPdfEndpoint.HandleAsync</c>.
/// </summary>
public sealed class HandleAsyncTests
{
    private readonly RecurringItemForecastWorkflowFake _workflow = new();
    private readonly RecurringItemForecastPdfRendererFake _renderer = new();
    private readonly TimeProviderFake _timeProvider = new() { UtcNow = new DateTimeOffset(2026, 10, 31, 22, 30, 0, TimeSpan.Zero) };
    private readonly LoggerFake<RecurringItemForecastPdfEndpointUnderTest> _logger = new();
    private readonly RecurringItemForecastPdfEndpointUnderTest _endpoint;

    public HandleAsyncTests()
    {
        // 22:30 UTC on 31 October is already November in Berlin; the local month counts.
        _timeProvider.UseLocalOffset(TimeSpan.FromHours(2));
        _endpoint = new RecurringItemForecastPdfEndpointUnderTest(_workflow, _renderer, _timeProvider, _logger);
    }

    [Fact]
    public async Task ForecastsSixMonthsFromTheCurrentLocalMonth()
    {
        await _endpoint.HandleAsync(Xunit.TestContext.Current.CancellationToken);

        Assert.Equal([(new DateOnly(2026, 11, 1), 6)], _workflow.Requests);
    }

    [Fact]
    public async Task RendersTheForecastWithTheCurrentLocalTimeAsCreationTime()
    {
        RecurringItemForecastData forecast = new() { Months = [TestRecurringItemForecasts.CreateEmpty(new DateOnly(2026, 11, 1))] };
        _workflow.Forecast = forecast;

        await _endpoint.HandleAsync(Xunit.TestContext.Current.CancellationToken);

        (RecurringItemForecastData renderedForecast, DateTimeOffset createdAt) = Assert.Single(_renderer.Renderings);
        Assert.Same(forecast, renderedForecast);
        Assert.Equal(new DateTimeOffset(2026, 11, 1, 0, 30, 0, TimeSpan.FromHours(2)), createdAt);
    }

    [Fact]
    public async Task SendsThePdfAsDownloadNamedAfterTheFirstMonth()
    {
        _renderer.Pdf = [1, 2, 3];

        IResult result = await _endpoint.HandleAsync(Xunit.TestContext.Current.CancellationToken);

        FileContentHttpResult file = Assert.IsType<FileContentHttpResult>(result);
        Assert.Equal("fixkosten-prognose-2026-11.pdf", file.FileDownloadName);
        Assert.Equal("application/pdf", file.ContentType);
        Assert.Equal([1, 2, 3], file.FileContents.ToArray());
    }

    [Fact]
    public async Task LogsTheDownload()
    {
        await _endpoint.HandleAsync(Xunit.TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
    }
}
