using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using RecurringItemForecastWorkflowUnderTest =
    GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.RecurringItemForecastWorkflow;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Workflows.RecurringItemForecast.RecurringItemForecastWorkflow;

/// <summary>
/// Runs a <c>RecurringItemForecastWorkflow</c> against in-memory managers and a recording logger, and offers a small
/// category tree: "Wohnen" with the sub-category "Miete" for expenses, "Gehalt" for income.
/// </summary>
internal sealed class RecurringItemForecastWorkflowProbe
{
    public RecurringItemForecastWorkflowProbe()
    {
        Wohnen = new Category { Name = "Wohnen", Type = CategoryType.Expense, Color = "#ff4d6d" };
        Miete = new Category { Name = "Miete", Type = CategoryType.Expense, ParentId = Wohnen.Id };
        Gehalt = new Category { Name = "Gehalt", Type = CategoryType.Income, Color = "#00ffa3" };
        CategoryManager.Add(Wohnen);
        CategoryManager.Add(Miete);
        CategoryManager.Add(Gehalt);
        Workflow = new RecurringItemForecastWorkflowUnderTest(RecurringItemManager, CategoryManager, Logger);
    }

    public Category Wohnen { get; }

    public Category Miete { get; }

    public Category Gehalt { get; }

    public RecurringItemManagerFake RecurringItemManager { get; } = new();

    public CategoryManagerFake CategoryManager { get; } = new();

    public LoggerFake<RecurringItemForecastWorkflowUnderTest> Logger { get; } = new();

    public RecurringItemForecastWorkflowUnderTest Workflow { get; }

    /// <summary>
    /// Stores and returns an active recurring item of the given category.
    /// </summary>
    public RecurringItem AddItem(
        string name,
        Category category,
        decimal amount,
        RecurringInterval interval = RecurringInterval.Monthly,
        int dueDay = 1,
        int startMonth = 0)
    {
        RecurringItem item = new()
        {
            Name = name,
            Type = category.Type,
            CategoryId = category.Id,
            Amount = amount,
            Interval = interval,
            DueDay = dueDay,
            StartMonth = startMonth
        };
        RecurringItemManager.Add(item);
        return item;
    }
}
