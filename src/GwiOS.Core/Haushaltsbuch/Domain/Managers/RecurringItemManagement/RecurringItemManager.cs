using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement;

internal sealed class RecurringItemManager(
    IRecurringItemRepository recurringItemRepository,
    IRecurringItemValidator recurringItemValidator,
    ILogger<RecurringItemManager> logger) : IRecurringItemManager
{
    private readonly IRecurringItemRepository _recurringItemRepository = recurringItemRepository;
    private readonly IRecurringItemValidator _recurringItemValidator = recurringItemValidator;
    private readonly ILogger<RecurringItemManager> _logger = logger;

    public async Task<RecurringItem> CreateRecurringItemAsync(
        RecurringItem recurringItem,
        CancellationToken cancellationToken = default)
    {
        ThrowIfInvalid(recurringItem);
        await _recurringItemRepository.InsertAsync(recurringItem, cancellationToken);
        _logger.LogInformation("Recurring item created.", CreateRecurringItemIdData(recurringItem.Id));
        return recurringItem;
    }

    public async Task UpdateRecurringItemAsync(RecurringItem recurringItem, CancellationToken cancellationToken = default)
    {
        ThrowIfInvalid(recurringItem);
        await _recurringItemRepository.UpdateAsync(recurringItem, cancellationToken);
        _logger.LogInformation("Recurring item updated.", CreateRecurringItemIdData(recurringItem.Id));
    }

    public async Task<List<RecurringItem>> GetAllRecurringItemsAsync(CancellationToken cancellationToken = default)
        => await _recurringItemRepository.GetAllAsync(cancellationToken);

    public async Task DeleteRecurringItemAsync(Guid recurringItemId, CancellationToken cancellationToken = default)
    {
        await _recurringItemRepository.DeleteAsync(recurringItemId, cancellationToken);
        _logger.LogInformation("Recurring item deleted.", CreateRecurringItemIdData(recurringItemId));
    }

    private void ThrowIfInvalid(RecurringItem recurringItem)
    {
        if (!_recurringItemValidator.IsValid(recurringItem, out List<string> validationErrors))
        {
            throw new RecurringItemValidationException(recurringItem.Id, validationErrors);
        }
    }

    private static Dictionary<string, string> CreateRecurringItemIdData(Guid recurringItemId)
        => new() { ["RecurringItemId"] = recurringItemId.ToString() };
}
