using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.HaushaltsbuchOverview.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.HaushaltsbuchOverview.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Workflows.HaushaltsbuchOverview;

internal sealed class HaushaltsbuchOverviewWorkflow(
    ICategoryManager categoryManager,
    IRecurringItemManager recurringItemManager,
    IBookingManager bookingManager) : IHaushaltsbuchOverviewWorkflow
{
    private readonly ICategoryManager _categoryManager = categoryManager;
    private readonly IRecurringItemManager _recurringItemManager = recurringItemManager;
    private readonly IBookingManager _bookingManager = bookingManager;

    public async Task<HaushaltsbuchOverviewData> GetOverviewAsync(
        DateOnly fromDate,
        DateOnly toDate,
        CancellationToken cancellationToken = default)
        => new()
        {
            Categories = await _categoryManager.GetAllCategoriesAsync(cancellationToken),
            RecurringItems = await _recurringItemManager.GetAllRecurringItemsAsync(cancellationToken),
            Bookings = await _bookingManager.GetBookingsInRangeAsync(fromDate, toDate, cancellationToken)
        };
}
