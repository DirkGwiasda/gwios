using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Haushaltsbuch.Domain.Managers.RecurringItemManagement.RecurringItemManager;

/// <summary>
/// Covers <c>RecurringItemManager.DeleteRecurringItemAsync</c>.
/// </summary>
public sealed class DeleteRecurringItemAsyncTests
{
    private readonly RecurringItemManagerProbe _probe = new();
    private readonly RecurringItem _miete = new()
    {
        Name = "Miete", Type = CategoryType.Expense, CategoryId = Guid.CreateVersion7(), Amount = 1150m,
        Interval = RecurringInterval.Monthly
    };
    private readonly RecurringItem _gehalt = new()
    {
        Name = "Gehalt", Type = CategoryType.Income, CategoryId = Guid.CreateVersion7(), Amount = 3200m,
        Interval = RecurringInterval.Monthly
    };

    [Fact]
    public async Task DeletesOnlyTheRecurringItemWithTheGivenId()
    {
        await _probe.RecurringItemRepository.InsertAsync(_miete, TestContext.Current.CancellationToken);
        await _probe.RecurringItemRepository.InsertAsync(_gehalt, TestContext.Current.CancellationToken);

        await _probe.RecurringItemManager.DeleteRecurringItemAsync(_miete.Id, TestContext.Current.CancellationToken);

        List<RecurringItem> stored =
            await _probe.RecurringItemRepository.GetAllAsync(TestContext.Current.CancellationToken);
        Assert.Same(_gehalt, Assert.Single(stored));
    }

    [Fact]
    public async Task Succeeds_WhenTheRecurringItemIsNotStored()
    {
        Exception? exception = await Record.ExceptionAsync(
            () => _probe.RecurringItemManager.DeleteRecurringItemAsync(
                _miete.Id,
                TestContext.Current.CancellationToken));

        Assert.Null(exception);
    }

    [Fact]
    public async Task LogsTheDeletionWithTheRecurringItemId()
    {
        await _probe.RecurringItemRepository.InsertAsync(_miete, TestContext.Current.CancellationToken);

        await _probe.RecurringItemManager.DeleteRecurringItemAsync(_miete.Id, TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Recurring item deleted.", logEntry.Message);
        Assert.Equal(_miete.Id.ToString(), logEntry.ContextData["RecurringItemId"]);
    }
}
