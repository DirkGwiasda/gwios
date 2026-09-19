using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.RecurringItemManagement.RecurringItemManager;

/// <summary>
/// Covers <c>RecurringItemManager.UpdateRecurringItemAsync</c>.
/// </summary>
public sealed class UpdateRecurringItemAsyncTests
{
    private readonly RecurringItemManagerProbe _probe = new();
    private readonly RecurringItem _recurringItem = new()
    {
        Name = "Miete", Type = CategoryType.Expense, CategoryId = Guid.CreateVersion7(), Amount = 1150m,
        Interval = RecurringInterval.Monthly
    };

    [Fact]
    public async Task StoresTheChangedRecurringItem_WhenItIsValid()
    {
        await _probe.RecurringItemRepository.InsertAsync(_recurringItem, TestContext.Current.CancellationToken);
        RecurringItem changed = new()
        {
            Id = _recurringItem.Id, Name = "Kaltmiete", Type = CategoryType.Expense,
            CategoryId = _recurringItem.CategoryId, Amount = 1200m, Interval = RecurringInterval.Monthly,
            Active = false
        };

        await _probe.RecurringItemManager.UpdateRecurringItemAsync(changed, TestContext.Current.CancellationToken);

        RecurringItem stored = Assert.Single(
            await _probe.RecurringItemRepository.GetAllAsync(TestContext.Current.CancellationToken));
        Assert.Equal("Kaltmiete", stored.Name);
        Assert.Equal(1200m, stored.Amount);
        Assert.False(stored.Active);
    }

    [Fact]
    public async Task ValidatesTheRecurringItem()
    {
        await _probe.RecurringItemRepository.InsertAsync(_recurringItem, TestContext.Current.CancellationToken);

        await _probe.RecurringItemManager.UpdateRecurringItemAsync(_recurringItem, TestContext.Current.CancellationToken);

        Assert.Same(_recurringItem, Assert.Single(_probe.RecurringItemValidator.ValidatedRecurringItems));
    }

    [Fact]
    public async Task ThrowsRecurringItemValidationExceptionWithTheValidationErrors_WhenTheRecurringItemIsInvalid()
    {
        await _probe.RecurringItemRepository.InsertAsync(_recurringItem, TestContext.Current.CancellationToken);
        _probe.RecurringItemValidator.ValidationErrors = ["First error.", "Second error."];

        RecurringItemValidationException exception = await Assert.ThrowsAsync<RecurringItemValidationException>(
            () => _probe.RecurringItemManager.UpdateRecurringItemAsync(
                _recurringItem,
                TestContext.Current.CancellationToken));

        Assert.Equal(_recurringItem.Id, exception.RecurringItemId);
        Assert.Equal(["First error.", "Second error."], exception.ValidationErrors);
    }

    [Fact]
    public async Task ThrowsRecurringItemNotFoundException_WhenTheRecurringItemIsNotStored()
    {
        await Assert.ThrowsAsync<RecurringItemNotFoundException>(
            () => _probe.RecurringItemManager.UpdateRecurringItemAsync(
                _recurringItem,
                TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LogsTheUpdateWithTheRecurringItemId()
    {
        await _probe.RecurringItemRepository.InsertAsync(_recurringItem, TestContext.Current.CancellationToken);

        await _probe.RecurringItemManager.UpdateRecurringItemAsync(_recurringItem, TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Recurring item updated.", logEntry.Message);
        Assert.Equal(_recurringItem.Id.ToString(), logEntry.ContextData["RecurringItemId"]);
    }
}
