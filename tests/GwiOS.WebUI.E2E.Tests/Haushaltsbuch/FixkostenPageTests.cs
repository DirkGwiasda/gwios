using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace GwiOS.WebUI.E2E.Tests.Haushaltsbuch;

/// <summary>
/// End-to-end coverage of the Fixkosten tab of the Haushaltsbuch, driven through a real browser against the real
/// GwiOS.WebUI.
/// </summary>
[Collection(GwiOSAppCollection.Name)]
public sealed class FixkostenPageTests(GwiOSAppFixture app) : IAsyncLifetime
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
    public async Task ShowsTheHeadingAndMonthlyEquivalentPanel()
    {
        await _page.GotoAsync($"{app.BaseUrl}/haushaltsbuch/fixkosten");

        await Expect(_page.Locator("h1")).ToHaveTextAsync("Haushaltsbuch");
        await Expect(_page.GetByText("Monatliches Äquivalent (aktiv)")).ToBeVisibleAsync();
    }

    [Fact]
    public async Task CreatesAndDeletesARecurringExpenseItem()
    {
        await using TestDataSeeder seeder = new();
        Category category = await seeder.CreateExpenseCategoryAsync();
        string name = TestDataSeeder.CreateUniqueName("e2e-fixkosten");

        await _page.GotoAsync($"{app.BaseUrl}/haushaltsbuch/fixkosten");
        await _page.GetByRole(AriaRole.Button, new() { Name = "+ Neuer wiederkehrender Posten" }).ClickAsync();
        await _page.Locator("#fix-edit-name").FillAsync(name);
        await _page.Locator("#fix-edit-category").SelectOptionAsync(category.Id.ToString());
        await _page.Locator("#fix-edit-amount").FillAsync("19.99");
        await _page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        ILocator row = _page.Locator("tr", new PageLocatorOptions { HasText = name });
        await Expect(row).ToBeVisibleAsync();
        await Expect(row).ToContainTextAsync("19,99");

        // The delete button asks for a second click to confirm; its title changes once clicked, so it is located
        // by position, not by its (changing) title.
        ILocator deleteButton = row.Locator(".gwios-table-actions button").Last;
        await deleteButton.ClickAsync();
        await deleteButton.ClickAsync();

        await Expect(row).Not.ToBeVisibleAsync();
    }
}
