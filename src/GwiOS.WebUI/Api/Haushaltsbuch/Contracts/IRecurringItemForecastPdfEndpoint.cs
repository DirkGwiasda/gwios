namespace GwiOS.WebUI.Api.Haushaltsbuch.Contracts;

/// <summary>
/// Handles <c>GET /api/haushaltsbuch/fixkosten-prognose</c>: sends the forecast of the recurring items for the current
/// and the following months as a PDF download.
/// </summary>
public interface IRecurringItemForecastPdfEndpoint
{
    /// <summary>
    /// How many months the forecast covers, starting with the current one; the Berichte tab shows the same months.
    /// </summary>
    const int MonthCount = 6;

    /// <summary>
    /// Creates the forecast from the current month on and returns it as a PDF download named after its first month,
    /// e.g. <c>fixkosten-prognose-2026-10.pdf</c>.
    /// </summary>
    Task<IResult> HandleAsync(CancellationToken cancellationToken);
}
