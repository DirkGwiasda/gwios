using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.CategoryDeletion.Contracts;

namespace GwiOS.Core.Haushaltsbuch.Domain.Workflows.CategoryDeletion;

internal sealed class CategoryDeletionWorkflow(
    ICategoryManager categoryManager,
    IRecurringItemManager recurringItemManager,
    IBookingManager bookingManager,
    ILogger<CategoryDeletionWorkflow> logger) : ICategoryDeletionWorkflow
{
    private readonly ICategoryManager _categoryManager = categoryManager;
    private readonly IRecurringItemManager _recurringItemManager = recurringItemManager;
    private readonly IBookingManager _bookingManager = bookingManager;
    private readonly ILogger<CategoryDeletionWorkflow> _logger = logger;

    public async Task DeleteCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        if (await IsInUseAsync(categoryId, cancellationToken))
        {
            _logger.LogWarning(
                "Category cannot be deleted because it is still in use.",
                new Dictionary<string, string> { ["CategoryId"] = categoryId.ToString() });
            throw new CategoryInUseException(categoryId);
        }

        await _categoryManager.DeleteCategoryAsync(categoryId, cancellationToken);
    }

    private async Task<bool> IsInUseAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        List<RecurringItem> recurringItems = await _recurringItemManager.GetAllRecurringItemsAsync(cancellationToken);
        if (recurringItems.Any(recurringItem => recurringItem.CategoryId == categoryId))
        {
            return true;
        }

        List<Booking> bookings = await _bookingManager.GetBookingsInRangeAsync(
            DateOnly.MinValue,
            DateOnly.MaxValue,
            cancellationToken);
        return bookings.Any(booking => booking.CategoryId == categoryId);
    }
}
