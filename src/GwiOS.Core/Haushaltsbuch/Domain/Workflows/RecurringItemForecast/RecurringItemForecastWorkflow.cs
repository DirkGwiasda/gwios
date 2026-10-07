using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast;

internal sealed class RecurringItemForecastWorkflow(
    IRecurringItemManager recurringItemManager,
    ICategoryManager categoryManager,
    ILogger<RecurringItemForecastWorkflow> logger) : IRecurringItemForecastWorkflow
{
    // Categories nest at most 5 levels deep; the limit only protects against a cycle in inconsistent data.
    private const int MaxCategoryDepth = 8;

    private readonly IRecurringItemManager _recurringItemManager = recurringItemManager;
    private readonly ICategoryManager _categoryManager = categoryManager;
    private readonly ILogger<RecurringItemForecastWorkflow> _logger = logger;

    public async Task<RecurringItemForecastData> GetForecastAsync(
        DateOnly firstMonth,
        int monthCount,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(monthCount, 1);

        List<RecurringItem> activeItems = [.. (await _recurringItemManager.GetAllRecurringItemsAsync(cancellationToken))
            .Where(item => item.Active)];
        Dictionary<Guid, Category> categoriesById = (await _categoryManager.GetAllCategoriesAsync(cancellationToken))
            .ToDictionary(category => category.Id);

        DateOnly start = new(firstMonth.Year, firstMonth.Month, 1);
        List<RecurringItemForecastMonth> months = [.. Enumerable.Range(0, monthCount)
            .Select(offset => CreateMonth(start.AddMonths(offset), activeItems, categoriesById))];

        _logger.LogDebug(
            "Recurring item forecast created.",
            new Dictionary<string, string>
            {
                ["FirstMonth"] = start.ToString("O"),
                ["MonthCount"] = monthCount.ToString()
            });
        return new RecurringItemForecastData { Months = months };
    }

    private RecurringItemForecastMonth CreateMonth(
        DateOnly month,
        List<RecurringItem> activeItems,
        Dictionary<Guid, Category> categoriesById)
    {
        List<RecurringItemForecastEntry> incomes = CreateEntries(month, activeItems, CategoryType.Income, categoriesById);
        List<RecurringItemForecastEntry> expenses = CreateEntries(month, activeItems, CategoryType.Expense, categoriesById);
        decimal incomeTotal = incomes.Sum(entry => entry.Amount);
        decimal expenseTotal = expenses.Sum(entry => entry.Amount);
        return new RecurringItemForecastMonth
        {
            Month = month,
            Incomes = incomes,
            Expenses = expenses,
            IncomeTotal = incomeTotal,
            ExpenseTotal = expenseTotal,
            Balance = incomeTotal - expenseTotal
        };
    }

    private List<RecurringItemForecastEntry> CreateEntries(
        DateOnly month,
        List<RecurringItem> activeItems,
        CategoryType type,
        Dictionary<Guid, Category> categoriesById)
        => [.. activeItems
            .Where(item => item.Type == type)
            .SelectMany(item => _recurringItemManager.GetDueDates(item, month)
                .Select(date => new RecurringItemForecastEntry
                {
                    RecurringItemId = item.Id,
                    Date = date,
                    Name = item.Name,
                    CategoryId = item.CategoryId,
                    CategoryPath = CollectCategoryPath(item.CategoryId, categoriesById),
                    Amount = item.Amount
                }))
            .OrderBy(entry => entry.Date)
            .ThenBy(entry => entry.Name, StringComparer.CurrentCulture)];

    private static List<string> CollectCategoryPath(Guid categoryId, Dictionary<Guid, Category> categoriesById)
    {
        List<string> path = [];
        Guid? currentId = categoryId;
        while ((currentId is Guid id) && categoriesById.TryGetValue(id, out Category? category) && (path.Count < MaxCategoryDepth))
        {
            path.Insert(0, category.Name);
            currentId = category.ParentId;
        }

        return path;
    }
}
