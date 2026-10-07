using System.Text;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace GwiOS.WebUI.E2E.Tests.Haushaltsbuch;

/// <summary>
/// End-to-end coverage of the Berichte tab of the Haushaltsbuch, driven through a real browser against the real
/// GwiOS.WebUI.
/// </summary>
[Collection(GwiOSAppCollection.Name)]
public sealed class BerichtePageTests(GwiOSAppFixture app) : IAsyncLifetime
{
    private IBrowserContext _context = default!;
    private IPage _page = default!;

    public async ValueTask InitializeAsync()
    {
        _context = await app.Browser!.NewContextAsync();
        _page = await _context.NewPageAsync();
    }

    public async ValueTask DisposeAsync()
        => await _context.DisposeAsync();

    [Fact]
    public async Task ShowsTheReportSections()
    {
        await app.OpenAsync(_page, "/haushaltsbuch/berichte");

        await Expect(_page.Locator("h1")).ToHaveTextAsync("Haushaltsbuch");
        await Expect(_page.GetByText("Soll / Ist")).ToBeVisibleAsync();
        await Expect(_page.GetByText("Ausgaben nach Kategorie")).ToBeVisibleAsync();
        await Expect(_page.GetByText("Monatsverlauf")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task SwitchesTheSelectedRange()
    {
        await app.OpenAsync(_page, "/haushaltsbuch/berichte");

        await _page.GetByRole(AriaRole.Button, new() { Name = "Letzte 6 Monate" }).ClickAsync();

        await Expect(_page.GetByRole(AriaRole.Button, new() { Name = "Letzte 6 Monate" }))
            .ToHaveAttributeAsync("aria-pressed", "true");
    }

    [Fact]
    public async Task ShowsAnActiveRecurringItemInTheForecastOfTheCurrentMonth()
    {
        await using TestDataSeeder seeder = new();
        Category category = await seeder.CreateExpenseCategoryAsync();
        IRecurringItemManager recurringItemManager = seeder.Services.GetRequiredService<IRecurringItemManager>();
        RecurringItem item = await recurringItemManager.CreateRecurringItemAsync(new RecurringItem
        {
            Name = TestDataSeeder.CreateUniqueName("e2e-forecast"),
            Type = CategoryType.Expense,
            CategoryId = category.Id,
            Amount = 12.34m,
            Interval = RecurringInterval.Monthly
        });
        try
        {
            await app.OpenAsync(_page, "/haushaltsbuch/berichte");

            ILocator currentMonth = _page.Locator(".gwios-forecast-month").First;
            await Expect(currentMonth.Locator("h3")).ToHaveTextAsync(DateTime.Now.ToString("MMMM yyyy", new System.Globalization.CultureInfo("de-DE")));
            await Expect(currentMonth.GetByText(item.Name)).ToBeVisibleAsync();
            await Expect(_page.Locator(".gwios-forecast-month")).ToHaveCountAsync(6);
        }
        finally
        {
            await recurringItemManager.DeleteRecurringItemAsync(item.Id);
        }
    }

    [Fact]
    public async Task DownloadsTheForecastAsPdf()
    {
        await app.OpenAsync(_page, "/haushaltsbuch/berichte");

        IDownload download = await _page.RunAndWaitForDownloadAsync(
            () => _page.GetByRole(AriaRole.Link, new() { Name = "Als PDF herunterladen" }).ClickAsync());

        Assert.Equal($"fixkosten-prognose-{DateTime.Now:yyyy-MM}.pdf", download.SuggestedFilename);
        await using Stream pdf = await download.CreateReadStreamAsync();
        byte[] header = new byte[4];
        await pdf.ReadExactlyAsync(header);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(header));
    }
}
