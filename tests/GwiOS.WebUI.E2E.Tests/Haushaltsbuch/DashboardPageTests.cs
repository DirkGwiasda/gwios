using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
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

    private static async Task CleanUpBookingAsync(TestDataSeeder seeder, string description)
    {
        IBookingManager bookingManager = seeder.Services.GetRequiredService<IBookingManager>();
        List<Booking> bookings = await bookingManager.GetBookingsInRangeAsync(DateOnly.MinValue, DateOnly.MaxValue);
        Booking booking = bookings.Single(candidate => candidate.Description == description);
        await bookingManager.DeleteBookingAsync(booking.Id);
    }
}
