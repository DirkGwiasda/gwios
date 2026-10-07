using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts.Models;
using GwiOS.WebUI.Api.Haushaltsbuch.Contracts;
using GwiOS.WebUI.Pdf.Contracts;

namespace GwiOS.WebUI.Api.Haushaltsbuch;

internal sealed class RecurringItemForecastPdfEndpoint(
    IRecurringItemForecastWorkflow recurringItemForecastWorkflow,
    IRecurringItemForecastPdfRenderer recurringItemForecastPdfRenderer,
    TimeProvider timeProvider,
    ILogger<RecurringItemForecastPdfEndpoint> logger) : IRecurringItemForecastPdfEndpoint
{
    private const string PdfContentType = "application/pdf";

    private readonly IRecurringItemForecastWorkflow _recurringItemForecastWorkflow = recurringItemForecastWorkflow;
    private readonly IRecurringItemForecastPdfRenderer _recurringItemForecastPdfRenderer = recurringItemForecastPdfRenderer;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<RecurringItemForecastPdfEndpoint> _logger = logger;

    public async Task<IResult> HandleAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset now = _timeProvider.GetLocalNow();
        DateOnly currentMonth = new(now.Year, now.Month, 1);
        RecurringItemForecastData forecast = await _recurringItemForecastWorkflow.GetForecastAsync(
            currentMonth,
            IRecurringItemForecastPdfEndpoint.MonthCount,
            cancellationToken);
        byte[] pdf = _recurringItemForecastPdfRenderer.Render(forecast, now);
        _logger.LogInformation("Recurring item forecast PDF downloaded.");
        return Results.File(pdf, PdfContentType, $"fixkosten-prognose-{currentMonth:yyyy-MM}.pdf");
    }
}
