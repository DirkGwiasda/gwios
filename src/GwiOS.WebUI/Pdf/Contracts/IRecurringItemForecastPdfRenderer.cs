using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts.Models;

namespace GwiOS.WebUI.Pdf.Contracts;

/// <summary>
/// Turns a forecast of the recurring items into a printable PDF document.
/// </summary>
public interface IRecurringItemForecastPdfRenderer
{
    /// <summary>
    /// Returns the PDF document of the given forecast, one section per month with its income, expenses and balance.
    /// </summary>
    /// <param name="forecast">The forecast to render.</param>
    /// <param name="createdAt">The point in time printed as creation time, in the time zone to show it in.</param>
    byte[] Render(RecurringItemForecastData forecast, DateTimeOffset createdAt);
}
