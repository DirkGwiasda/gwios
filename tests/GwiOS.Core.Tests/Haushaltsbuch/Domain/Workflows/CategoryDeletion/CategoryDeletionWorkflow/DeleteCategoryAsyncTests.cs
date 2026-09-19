using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using CategoryDeletionWorkflowUnderTest = GwiOS.Core.Haushaltsbuch.Domain.Workflows.CategoryDeletion.CategoryDeletionWorkflow;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Workflows.CategoryDeletion.CategoryDeletionWorkflow;

/// <summary>
/// Covers <c>CategoryDeletionWorkflow.DeleteCategoryAsync</c>.
/// </summary>
public sealed class DeleteCategoryAsyncTests
{
    private readonly CategoryManagerFake _categoryManager = new();
    private readonly RecurringItemManagerFake _recurringItemManager = new();
    private readonly BookingManagerFake _bookingManager = new();
    private readonly LoggerFake<CategoryDeletionWorkflowUnderTest> _logger = new();
    private readonly CategoryDeletionWorkflowUnderTest _workflow;
    private readonly Category _category = new() { Name = "Wohnen", Type = CategoryType.Expense, Color = "#ff4d6d" };

    public DeleteCategoryAsyncTests()
    {
        _workflow = new CategoryDeletionWorkflowUnderTest(
            _categoryManager,
            _recurringItemManager,
            _bookingManager,
            _logger);
        _categoryManager.Add(_category);
    }

    [Fact]
    public async Task DeletesTheCategory_WhenItIsNotUsedAnywhere()
    {
        await _workflow.DeleteCategoryAsync(_category.Id, TestContext.Current.CancellationToken);

        Assert.Equal([_category.Id], _categoryManager.DeletedCategoryIds);
    }

    [Fact]
    public async Task ThrowsCategoryInUseExceptionAndKeepsTheCategory_WhenARecurringItemUsesIt()
    {
        _recurringItemManager.Add(new RecurringItem
        {
            Name = "Miete", Type = CategoryType.Expense, CategoryId = _category.Id, Amount = 1150m,
            Interval = RecurringInterval.Monthly
        });

        CategoryInUseException exception = await Assert.ThrowsAsync<CategoryInUseException>(
            () => _workflow.DeleteCategoryAsync(_category.Id, TestContext.Current.CancellationToken));

        Assert.Equal(_category.Id, exception.CategoryId);
        Assert.Empty(_categoryManager.DeletedCategoryIds);
    }

    [Fact]
    public async Task ThrowsCategoryInUseExceptionAndKeepsTheCategory_WhenABookingUsesIt()
    {
        _bookingManager.Add(new Booking
        {
            Date = new DateOnly(2026, 9, 1), Type = CategoryType.Expense, CategoryId = _category.Id, Amount = 58m,
            Status = BookingStatus.Booked
        });

        CategoryInUseException exception = await Assert.ThrowsAsync<CategoryInUseException>(
            () => _workflow.DeleteCategoryAsync(_category.Id, TestContext.Current.CancellationToken));

        Assert.Equal(_category.Id, exception.CategoryId);
        Assert.Empty(_categoryManager.DeletedCategoryIds);
    }

    [Fact]
    public async Task ThrowsCategoryHasChildrenException_WhenAnotherCategoryHasItAsItsParent()
    {
        _categoryManager.Add(new Category { Name = "Miete", Type = CategoryType.Expense, ParentId = _category.Id });

        await Assert.ThrowsAsync<CategoryHasChildrenException>(
            () => _workflow.DeleteCategoryAsync(_category.Id, TestContext.Current.CancellationToken));

        Assert.Empty(_categoryManager.DeletedCategoryIds);
    }

    [Fact]
    public async Task LogsAWarning_WhenTheCategoryIsInUse()
    {
        _bookingManager.Add(new Booking
        {
            Date = new DateOnly(2026, 9, 1), Type = CategoryType.Expense, CategoryId = _category.Id, Amount = 58m,
            Status = BookingStatus.Booked
        });

        await Assert.ThrowsAsync<CategoryInUseException>(
            () => _workflow.DeleteCategoryAsync(_category.Id, TestContext.Current.CancellationToken));

        Assert.Single(_logger.Entries);
    }
}
