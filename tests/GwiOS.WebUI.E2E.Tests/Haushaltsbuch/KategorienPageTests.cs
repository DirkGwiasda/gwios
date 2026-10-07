using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace GwiOS.WebUI.E2E.Tests.Haushaltsbuch;

/// <summary>
/// End-to-end coverage of the Kategorien tab of the Haushaltsbuch, driven through a real browser against the real
/// GwiOS.WebUI.
/// </summary>
[Collection(GwiOSAppCollection.Name)]
public sealed class KategorienPageTests(GwiOSAppFixture app) : IAsyncLifetime
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
    public async Task ShowsTheIncomeAndExpenseTrees()
    {
        await app.OpenAsync(_page, "/haushaltsbuch/kategorien");

        await Expect(_page.Locator("h1")).ToHaveTextAsync("Haushaltsbuch");
        await Expect(_page.GetByText("Einnahmen", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(_page.GetByText("Ausgaben", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Fact]
    public async Task CreatesAndDeletesAnExpenseRootCategory()
    {
        string name = TestDataSeeder.CreateUniqueName("e2e-category");
        await app.OpenAsync(_page, "/haushaltsbuch/kategorien");

        ILocator expenseSection = _page.Locator("section", new PageLocatorOptions { HasText = "Ausgaben" });
        await expenseSection.GetByRole(AriaRole.Button, new() { Name = "+ Hauptkategorie" }).ClickAsync();
        await _page.Locator("#category-edit-name").FillAsync(name);
        await _page.GetByRole(AriaRole.Button, new() { Name = "Speichern" }).ClickAsync();

        ILocator categoryRow = _page.Locator(".gwios-category-row", new PageLocatorOptions { HasText = name });
        await Expect(categoryRow).ToBeVisibleAsync();

        // The delete button asks for a second click to confirm; its title changes once clicked, so it is located
        // by position, not by its (changing) title.
        ILocator deleteButton = categoryRow.Locator(".gwios-category-actions button").Last;
        await deleteButton.ClickAsync();
        await deleteButton.ClickAsync();

        await Expect(categoryRow).Not.ToBeVisibleAsync();
    }
}
