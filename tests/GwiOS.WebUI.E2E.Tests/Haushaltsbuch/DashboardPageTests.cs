using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using System.Text.RegularExpressions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringBookingGeneration.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace GwiOS.WebUI.E2E.Tests.Haushaltsbuch;

/// <summary>
/// End-to-end coverage of the Dashboard tab of the Haushaltsbuch, driven through a real browser against the real
/// GwiOS.WebUI.
/// </summary>
[Collection(GwiOSAppCollection.Name)]
public sealed class DashboardPageTests(GwiOSAppFixture app) : IAsyncLifetime
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
    public async Task ShowsTheHeadingAndKpiTiles()
    {
        await _page.GotoAsync($"{app.BaseUrl}/haushaltsbuch/dashboard");

        await Expect(_page.Locator("h1")).ToHaveTextAsync("Haushaltsbuch");
        await Expect(_page.GetByText("Gesamteinnahmen")).ToBeVisibleAsync();
        await Expect(_page.GetByText("Gesamtausgaben")).ToBeVisibleAsync();
        await Expect(_page.GetByText("Verfügbarer Saldo")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task CreatesANewBookingAndShowsItInTheTable()
    {
        await using TestDataSeeder seeder = new();
        Category category = await seeder.CreateExpenseCategoryAsync();
        string description = TestDataSeeder.CreateUniqueName("e2e-booking");

        await _page.GotoAsync($"{app.BaseUrl}/haushaltsbuch/dashboard");
        await _page.GetByRole(AriaRole.Button, new() { Name = "+ Neue Buchung" }).ClickAsync();
        await _page.Locator("#booking-edit-amount").FillAsync("42.50");
        await _page.Locator("#booking-edit-category").SelectOptionAsync(category.Id.ToString());
        await _page.Locator("#booking-edit-description").FillAsync(description);
        await _page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        ILocator row = _page.Locator("tr", new PageLocatorOptions { HasText = description });
        await Expect(row).ToBeVisibleAsync();
        await Expect(row).ToContainTextAsync("42,50");

        await CleanUpBookingAsync(seeder, description);
    }

    [Fact]
    public async Task GenerateFixBookingsTakesOverOnlyTheRecurringItemsLeftSelectedInTheDialog()
    {
        await using TestDataSeeder seeder = new();
        Category category = await seeder.CreateExpenseCategoryAsync();
        RecurringItem keptItem = await CreateMonthlyRecurringItemAsync(seeder, category);
        RecurringItem deselectedItem = await CreateMonthlyRecurringItemAsync(seeder, category);
        await MarkCurrentMonthAsReviewedAsync(seeder);

        try
        {
            await _page.GotoAsync($"{app.BaseUrl}/haushaltsbuch/dashboard");
            ILocator dialog = await OpenFixBookingSelectionAsync();
            await Expect(dialog.GetByRole(AriaRole.Checkbox, new() { Name = keptItem.Name })).ToBeCheckedAsync();
            await Expect(dialog.GetByRole(AriaRole.Checkbox, new() { Name = deselectedItem.Name })).ToBeCheckedAsync();
            await UncheckAllProposalsExceptAsync(dialog, keptItem.Name);
            await dialog.GetByRole(AriaRole.Button, new() { Name = "Übernehmen" }).ClickAsync();

            await Expect(dialog).ToBeHiddenAsync();
            await Expect(_page.Locator("tr", new PageLocatorOptions { HasText = keptItem.Name })).ToBeVisibleAsync();
            await Expect(_page.Locator("tr", new PageLocatorOptions { HasText = deselectedItem.Name })).ToHaveCountAsync(0);
        }
        finally
        {
            await CleanUpRecurringItemsAsync(seeder, keptItem, deselectedItem);
        }
    }

    [Fact]
    public async Task GenerateFixBookingsCreatesNothingWhenTheDialogIsCancelled()
    {
        await using TestDataSeeder seeder = new();
        Category category = await seeder.CreateExpenseCategoryAsync();
        RecurringItem item = await CreateMonthlyRecurringItemAsync(seeder, category);
        await MarkCurrentMonthAsReviewedAsync(seeder);

        try
        {
            await _page.GotoAsync($"{app.BaseUrl}/haushaltsbuch/dashboard");
            ILocator dialog = await OpenFixBookingSelectionAsync();
            await dialog.GetByRole(AriaRole.Button, new() { Name = "Abbrechen" }).ClickAsync();

            await Expect(dialog).ToBeHiddenAsync();
            await Expect(_page.Locator("tr", new PageLocatorOptions { HasText = item.Name })).ToHaveCountAsync(0);
        }
        finally
        {
            await CleanUpRecurringItemsAsync(seeder, item);
        }
    }

    private async Task<ILocator> OpenFixBookingSelectionAsync()
    {
        Regex generateButtonName = new("^Fixbuchungen für .+ generieren");
        await _page.GetByRole(AriaRole.Button, new() { NameRegex = generateButtonName }).ClickAsync();
        ILocator dialog = _page.GetByRole(AriaRole.Dialog);
        await Expect(dialog).ToBeVisibleAsync();
        return dialog;
    }

    // Other recurring items may exist in the shared test database; leaving only the own one selected keeps the test
    // from creating bookings it would not clean up.
    private static async Task UncheckAllProposalsExceptAsync(ILocator dialog, string keptName)
    {
        foreach (ILocator checkbox in await dialog.GetByRole(AriaRole.Checkbox).AllAsync())
        {
            string label = await checkbox.Locator("xpath=..").InnerTextAsync();
            if (!label.Contains(keptName))
            {
                await checkbox.UncheckAsync();
            }
        }
    }

    private static async Task<RecurringItem> CreateMonthlyRecurringItemAsync(TestDataSeeder seeder, Category category)
    {
        IRecurringItemManager recurringItemManager = seeder.Services.GetRequiredService<IRecurringItemManager>();
        RecurringItem item = new()
        {
            Name = TestDataSeeder.CreateUniqueName("e2e-recurring"),
            Type = CategoryType.Expense,
            CategoryId = category.Id,
            Amount = 12.34m,
            Interval = RecurringInterval.Monthly
        };
        return await recurringItemManager.CreateRecurringItemAsync(item);
    }

    // Without this, the automatic month review dialog would open instead and cover the button under test.
    private static async Task MarkCurrentMonthAsReviewedAsync(TestDataSeeder seeder)
    {
        IRecurringBookingGenerationWorkflow workflow =
            seeder.Services.GetRequiredService<IRecurringBookingGenerationWorkflow>();
        await workflow.ConfirmAsync(DateOnly.FromDateTime(DateTime.Now), []);
    }

    private static async Task CleanUpRecurringItemsAsync(TestDataSeeder seeder, params RecurringItem[] items)
    {
        IBookingManager bookingManager = seeder.Services.GetRequiredService<IBookingManager>();
        IRecurringItemManager recurringItemManager = seeder.Services.GetRequiredService<IRecurringItemManager>();
        HashSet<Guid> itemIds = [.. items.Select(item => item.Id)];
        List<Booking> bookings = await bookingManager.GetBookingsInRangeAsync(DateOnly.MinValue, DateOnly.MaxValue);
        foreach (Booking booking in bookings.Where(booking => (booking.RecurringItemId is Guid id) && itemIds.Contains(id)))
        {
            await bookingManager.DeleteBookingAsync(booking.Id);
        }

        foreach (RecurringItem item in items)
        {
            await recurringItemManager.DeleteRecurringItemAsync(item.Id);
        }
    }

    private static async Task CleanUpBookingAsync(TestDataSeeder seeder, string description)
    {
        IBookingManager bookingManager = seeder.Services.GetRequiredService<IBookingManager>();
        List<Booking> bookings = await bookingManager.GetBookingsInRangeAsync(DateOnly.MinValue, DateOnly.MaxValue);
        Booking booking = bookings.Single(candidate => candidate.Description == description);
        await bookingManager.DeleteBookingAsync(booking.Id);
    }
}
