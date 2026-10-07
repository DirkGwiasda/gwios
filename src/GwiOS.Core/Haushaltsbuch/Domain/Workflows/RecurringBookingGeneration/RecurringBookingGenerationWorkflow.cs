using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.MonthReviewManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringBookingGeneration.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringBookingGeneration.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringBookingGeneration;

internal sealed class RecurringBookingGenerationWorkflow(
    IRecurringItemManager recurringItemManager,
    IBookingManager bookingManager,
    IMonthReviewManager monthReviewManager,
    ILogger<RecurringBookingGenerationWorkflow> logger) : IRecurringBookingGenerationWorkflow
{
    private readonly IRecurringItemManager _recurringItemManager = recurringItemManager;
    private readonly IBookingManager _bookingManager = bookingManager;
    private readonly IMonthReviewManager _monthReviewManager = monthReviewManager;
    private readonly ILogger<RecurringBookingGenerationWorkflow> _logger = logger;

    public async Task<bool> NeedsReviewAsync(DateOnly month, CancellationToken cancellationToken = default)
        => !await _monthReviewManager.IsReviewedAsync(FirstDayOf(month), cancellationToken);

    public async Task<List<ProposedBooking>> GetProposedBookingsAsync(
        DateOnly month,
        CancellationToken cancellationToken = default)
    {
        DateOnly firstDay = FirstDayOf(month);
        DateOnly lastDay = LastDayOf(firstDay);

        List<RecurringItem> activeItems = [.. (await _recurringItemManager.GetAllRecurringItemsAsync(cancellationToken))
            .Where(item => item.Active)];
        List<Booking> existingBookings =
            await _bookingManager.GetBookingsInRangeAsync(firstDay, lastDay, cancellationToken);
        HashSet<(Guid RecurringItemId, DateOnly Date)> alreadyBooked = [.. existingBookings
            .Where(booking => booking.RecurringItemId is not null)
            .Select(booking => (booking.RecurringItemId!.Value, booking.Date))];

        List<ProposedBooking> proposedBookings = [];
        foreach (RecurringItem item in activeItems)
        {
            foreach (DateOnly date in _recurringItemManager.GetDueDates(item, firstDay))
            {
                if (alreadyBooked.Contains((item.Id, date)))
                {
                    continue;
                }

                proposedBookings.Add(new ProposedBooking
                {
                    RecurringItemId = item.Id,
                    Name = item.Name,
                    Type = item.Type,
                    CategoryId = item.CategoryId,
                    Date = date,
                    Amount = item.Amount
                });
            }
        }

        return proposedBookings;
    }

    public async Task ConfirmAsync(
        DateOnly month,
        IReadOnlyList<ProposedBooking> selectedBookings,
        CancellationToken cancellationToken = default)
    {
        foreach (ProposedBooking proposedBooking in selectedBookings)
        {
            Booking booking = new()
            {
                Date = proposedBooking.Date,
                Type = proposedBooking.Type,
                CategoryId = proposedBooking.CategoryId,
                Description = proposedBooking.Name,
                Amount = proposedBooking.Amount,
                Status = BookingStatus.Pending,
                RecurringItemId = proposedBooking.RecurringItemId
            };
            await _bookingManager.CreateBookingAsync(booking, cancellationToken);
        }

        await _monthReviewManager.MarkReviewedAsync(FirstDayOf(month), cancellationToken);
        _logger.LogInformation(
            "Recurring bookings confirmed for month.",
            new Dictionary<string, string>
            {
                ["Month"] = FirstDayOf(month).ToString("O"),
                ["ConfirmedCount"] = selectedBookings.Count.ToString()
            });
    }

    private static DateOnly FirstDayOf(DateOnly month)
        => new(month.Year, month.Month, 1);

    private static DateOnly LastDayOf(DateOnly firstDayOfMonth)
        => new(firstDayOfMonth.Year, firstDayOfMonth.Month, DateTime.DaysInMonth(firstDayOfMonth.Year, firstDayOfMonth.Month));
}
