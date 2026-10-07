using System.Globalization;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts.Models;
using GwiOS.WebUI.Pdf.Contracts;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GwiOS.WebUI.Pdf;

internal sealed class RecurringItemForecastPdfRenderer(
    IQuestPdfConfigurator questPdfConfigurator,
    ILogger<RecurringItemForecastPdfRenderer> logger) : IRecurringItemForecastPdfRenderer
{
    private const string FontFamily = "Inter";

    // Print colors: dark text on white instead of the dark theme of the app, the accent blue for headings.
    private const string TextColor = "#1d2533";
    private const string MutedColor = "#5b6b80";
    private const string AccentColor = "#1f5fa8";
    private const string IncomeColor = "#0a7d50";
    private const string ExpenseColor = "#b3261e";
    private const string RuleColor = "#d5dde8";
    private const string HeaderBackground = "#eef3f9";

    // A month starts on a new page unless its heading, the income heading and a few rows still fit.
    private const float MinimumMonthStartHeight = 140;

    private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-DE");

    private readonly IQuestPdfConfigurator _questPdfConfigurator = questPdfConfigurator;
    private readonly ILogger<RecurringItemForecastPdfRenderer> _logger = logger;

    public byte[] Render(RecurringItemForecastData forecast, DateTimeOffset createdAt)
    {
        _questPdfConfigurator.EnsureConfigured();
        byte[] pdf = Document.Create(document => document.Page(page => ComposePage(page, forecast, createdAt)))
            .GeneratePdf();
        _logger.LogDebug(
            "Recurring item forecast PDF rendered.",
            new Dictionary<string, string>
            {
                ["MonthCount"] = forecast.Months.Count.ToString(),
                ["Bytes"] = pdf.Length.ToString()
            });
        return pdf;
    }

    private static void ComposePage(PageDescriptor page, RecurringItemForecastData forecast, DateTimeOffset createdAt)
    {
        page.Size(PageSizes.A4);
        page.Margin(1.8f, Unit.Centimetre);
        page.DefaultTextStyle(style => style.FontFamily(FontFamily).FontSize(9.5f).FontColor(TextColor));
        page.Header().ShowOnce().Element(header => ComposeHeader(header, forecast, createdAt));
        page.Content().PaddingTop(14).Column(column =>
        {
            column.Spacing(20);
            foreach (RecurringItemForecastMonth month in forecast.Months)
            {
                column.Item().Element(container => ComposeMonth(container, month));
            }
        });
        page.Footer().Element(ComposeFooter);
    }

    private static void ComposeHeader(IContainer container, RecurringItemForecastData forecast, DateTimeOffset createdAt)
        => container.BorderBottom(1).BorderColor(AccentColor).PaddingBottom(8).Column(column =>
        {
            column.Item().Text("Fixkosten-Prognose").FontSize(18).Bold().FontColor(AccentColor);
            column.Item().Text(
                $"{FormatPeriod(forecast)} · aktive Fixposten · erstellt am {createdAt:dd.MM.yyyy} um {createdAt:HH:mm} Uhr")
                .FontSize(9).FontColor(MutedColor);
        });

    private static void ComposeMonth(IContainer container, RecurringItemForecastMonth month)
        => container.EnsureSpace(MinimumMonthStartHeight).Column(column =>
        {
            column.Spacing(6);
            column.Item().Text(FormatMonth(month.Month)).FontSize(13).SemiBold().FontColor(AccentColor);
            column.Item().Element(section => ComposeSection(section, "Einnahmen", month.Incomes, month.IncomeTotal, IncomeColor));
            column.Item().Element(section => ComposeSection(section, "Ausgaben", month.Expenses, month.ExpenseTotal, ExpenseColor));
            column.Item().AlignRight().Text(text =>
            {
                text.Span("Saldo  ").SemiBold();
                text.Span(FormatSignedAmount(month.Balance)).Bold().FontColor(month.Balance >= 0 ? IncomeColor : ExpenseColor);
            });
        });

    private static void ComposeSection(
        IContainer container,
        string title,
        IReadOnlyList<RecurringItemForecastEntry> entries,
        decimal total,
        string amountColor)
        => container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text(title).SemiBold();
                row.AutoItem().Text(FormatAmount(total)).SemiBold().FontColor(amountColor);
            });
            if (entries.Count == 0)
            {
                column.Item().PaddingTop(2).Text($"Keine fälligen {title}.").Italic().FontColor(MutedColor);
                return;
            }

            column.Item().PaddingTop(3).Element(table => ComposeTable(table, entries, amountColor));
        });

    private static void ComposeTable(IContainer container, IReadOnlyList<RecurringItemForecastEntry> entries, string amountColor)
        => container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(2.5f, Unit.Centimetre);
                columns.RelativeColumn(3);
                columns.RelativeColumn(4);
                columns.ConstantColumn(2.6f, Unit.Centimetre);
            });
            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("Datum");
                header.Cell().Element(HeaderCell).Text("Name");
                header.Cell().Element(HeaderCell).Text("Kategorie");
                header.Cell().Element(HeaderCell).AlignRight().Text("Betrag");
            });
            foreach (RecurringItemForecastEntry entry in entries)
            {
                table.Cell().Element(BodyCell).Text(entry.Date.ToString("dd.MM.yyyy", GermanCulture));
                table.Cell().Element(BodyCell).Text(entry.Name);
                table.Cell().Element(BodyCell).Text(FormatCategoryPath(entry.CategoryPath)).FontColor(MutedColor);
                table.Cell().Element(BodyCell).AlignRight().Text(FormatAmount(entry.Amount)).FontColor(amountColor);
            }
        });

    private static IContainer HeaderCell(IContainer container)
        => container.Background(HeaderBackground).PaddingVertical(3).PaddingHorizontal(4).DefaultTextStyle(
            style => style.FontSize(8.5f).SemiBold().FontColor(AccentColor));

    private static IContainer BodyCell(IContainer container)
        => container.BorderBottom(0.5f).BorderColor(RuleColor).PaddingVertical(3).PaddingHorizontal(4);

    private static void ComposeFooter(IContainer container)
        => container.AlignCenter().Text(text =>
        {
            text.DefaultTextStyle(style => style.FontSize(8).FontColor(MutedColor));
            text.Span("GwiOS · Haushaltsbuch · Seite ");
            text.CurrentPageNumber();
            text.Span(" von ");
            text.TotalPages();
        });

    private static string FormatPeriod(RecurringItemForecastData forecast)
        => forecast.Months.Count == 0
            ? string.Empty
            : $"{FormatMonth(forecast.Months[0].Month)} – {FormatMonth(forecast.Months[^1].Month)}";

    private static string FormatMonth(DateOnly month)
        => month.ToString("MMMM yyyy", GermanCulture);

    private static string FormatCategoryPath(IReadOnlyList<string> categoryPath)
        => categoryPath.Count == 0 ? "—" : string.Join(" › ", categoryPath);

    private static string FormatAmount(decimal value)
        => value.ToString("C", GermanCulture);

    private static string FormatSignedAmount(decimal value)
        => (value >= 0 ? "+ " : "– ") + Math.Abs(value).ToString("C", GermanCulture);
}
