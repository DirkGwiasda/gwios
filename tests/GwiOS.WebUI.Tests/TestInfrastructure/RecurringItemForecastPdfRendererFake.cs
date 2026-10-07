using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts.Models;
using GwiOS.WebUI.Pdf.Contracts;

namespace GwiOS.WebUI.Tests.TestInfrastructure;

/// <summary>
/// Stands in for <see cref="IRecurringItemForecastPdfRenderer"/>: returns <see cref="Pdf"/> and records what it was
/// asked to render.
/// </summary>
public sealed class RecurringItemForecastPdfRendererFake : IRecurringItemForecastPdfRenderer
{
    /// <summary>
    /// The bytes every rendering returns.
    /// </summary>
    public byte[] Pdf { get; set; } = "%PDF-1.4"u8.ToArray();

    /// <summary>
    /// The forecasts and creation times rendered, in call order.
    /// </summary>
    public List<(RecurringItemForecastData Forecast, DateTimeOffset CreatedAt)> Renderings { get; } = [];

    public byte[] Render(RecurringItemForecastData forecast, DateTimeOffset createdAt)
    {
        Renderings.Add((forecast, createdAt));
        return Pdf;
    }
}
