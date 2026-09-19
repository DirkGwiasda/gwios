using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.RecurringItemManagement.RecurringItemManager;

/// <summary>
/// Covers <c>RecurringItemManager.CreateRecurringItemAsync</c>.
/// </summary>
public sealed class CreateRecurringItemAsyncTests
{
    private readonly RecurringItemManagerProbe _probe = new();
    private readonly RecurringItem _recurringItem = new()
    {
        Name = "Miete", Type = CategoryType.Expense, CategoryId = Guid.CreateVersion7(), Amount = 1150m,
        Interval = RecurringInterval.Monthly
    };

    [Fact]
    public async Task StoresTheRecurringItem_WhenItIsValid()
    {
        await _probe.RecurringItemManager.CreateRecurringItemAsync(_recurringItem, TestContext.Current.CancellationToken);

        List<RecurringItem> stored =
            await _probe.RecurringItemRepository.GetAllAsync(TestContext.Current.CancellationToken);
        Assert.Same(_recurringItem, Assert.Single(stored));
    }

    [Fact]
    public async Task ReturnsTheStoredRecurringItem()
    {
        RecurringItem created = await _probe.RecurringItemManager.CreateRecurringItemAsync(
            _recurringItem,
            TestContext.Current.CancellationToken);

        Assert.Same(_recurringItem, created);
    }

    [Fact]
    public async Task ValidatesTheRecurringItem()
    {
        await _probe.RecurringItemManager.CreateRecurringItemAsync(_recurringItem, TestContext.Current.CancellationToken);

        Assert.Same(_recurringItem, Assert.Single(_probe.RecurringItemValidator.ValidatedRecurringItems));
    }

    [Fact]
    public async Task ThrowsRecurringItemValidationExceptionWithTheValidationErrors_WhenTheRecurringItemIsInvalid()
    {
        _probe.RecurringItemValidator.ValidationErrors = ["First error.", "Second error."];

        RecurringItemValidationException exception = await Assert.ThrowsAsync<RecurringItemValidationException>(
            () => _probe.RecurringItemManager.CreateRecurringItemAsync(
                _recurringItem,
                TestContext.Current.CancellationToken));

        Assert.Equal(_recurringItem.Id, exception.RecurringItemId);
        Assert.Equal(["First error.", "Second error."], exception.ValidationErrors);
    }

    [Fact]
    public async Task DoesNotStoreTheRecurringItem_WhenItIsInvalid()
    {
        _probe.RecurringItemValidator.ValidationErrors = ["Error."];

        await Assert.ThrowsAsync<RecurringItemValidationException>(
            () => _probe.RecurringItemManager.CreateRecurringItemAsync(
                _recurringItem,
                TestContext.Current.CancellationToken));

        Assert.Empty(await _probe.RecurringItemRepository.GetAllAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LogsTheCreationWithTheRecurringItemId()
    {
        await _probe.RecurringItemManager.CreateRecurringItemAsync(_recurringItem, TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Recurring item created.", logEntry.Message);
        Assert.Equal(_recurringItem.Id.ToString(), logEntry.ContextData["RecurringItemId"]);
    }
}
