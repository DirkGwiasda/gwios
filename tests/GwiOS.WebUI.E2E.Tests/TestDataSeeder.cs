using GwiOS.Core;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using Microsoft.Extensions.DependencyInjection;

namespace GwiOS.WebUI.E2E.Tests;

/// <summary>
/// Seeds a category directly through GwiOS.Core, for end-to-end tests that need one to already exist without
/// exercising the Kategorien tab for it, and deletes it again when the test is done. Also exposes the manager
/// scope, so a test can create and clean up further data (bookings, recurring items) it makes through the browser.
/// </summary>
public sealed class TestDataSeeder : IAsyncDisposable
{
    private const string TestConnectionStringVariableName = "GwiOS.DB.TestConnectionstring";

    private readonly ServiceProvider _serviceProvider;
    private readonly AsyncServiceScope _scope;
    private Category? _category;

    public TestDataSeeder()
    {
        ServiceCollection services = new();
        services.AddGwiOSCore(ReadTestConnectionString());
        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateAsyncScope();
    }

    /// <summary>
    /// The dependency injection scope the seeded data and any further test data is created through.
    /// </summary>
    public IServiceProvider Services
        => _scope.ServiceProvider;

    /// <summary>
    /// Creates and returns a new expense category with a unique name, prefixed so it is recognizable as test data.
    /// </summary>
    public async Task<Category> CreateExpenseCategoryAsync()
    {
        ICategoryManager categoryManager = Services.GetRequiredService<ICategoryManager>();
        _category = new Category { Name = CreateUniqueName("e2e-category"), Type = CategoryType.Expense, Color = "#ff4d6d" };
        await categoryManager.CreateCategoryAsync(_category);
        return _category;
    }

    /// <summary>
    /// Returns a name that no other test data uses, prefixed so leftovers are recognizable in the database.
    /// </summary>
    public static string CreateUniqueName(string prefix)
        => $"{prefix}-{Guid.NewGuid():N}";

    public async ValueTask DisposeAsync()
    {
        if (_category is not null)
        {
            ICategoryManager categoryManager = Services.GetRequiredService<ICategoryManager>();
            await categoryManager.DeleteCategoryAsync(_category.Id);
        }

        await _scope.DisposeAsync();
        await _serviceProvider.DisposeAsync();
    }

    private static string ReadTestConnectionString()
        => Environment.GetEnvironmentVariable(TestConnectionStringVariableName)
            ?? throw new InvalidOperationException(
                $"The environment variable '{TestConnectionStringVariableName}' must be set.");
}
