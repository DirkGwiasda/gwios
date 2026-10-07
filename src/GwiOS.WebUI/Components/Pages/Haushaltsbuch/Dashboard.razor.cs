using System.Globalization;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.HaushaltsbuchOverview.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.HaushaltsbuchOverview.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringBookingGeneration.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.RecurringBookingGeneration.Contracts.Models;
using GwiOS.WebUI.Components.GwiOS.Components;
using GwiOS.WebUI.StatusMessages.Contracts;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.Pages.Haushaltsbuch;

/// <summary>
/// Dashboard tab of the Haushaltsbuch: the KPI figures and bookings of the selected month, where bookings are
/// created, changed in a dialog and deleted, and where due recurring items are picked in a dialog to become bookings —
/// automatically on the first visit of an unreviewed month, or on demand through the "Fixbuchungen generieren" button.
/// </summary>
public partial class Dashboard
{
    private static readonly string[] MonthNames =
    [
        "Januar", "Februar", "März", "April", "Mai", "Juni",
        "Juli", "August", "September", "Oktober", "November", "Dezember"
    ];
    private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-DE");

    private DateOnly _month;
    private List<Category> _categories = [];
    private List<Booking> _bookings = [];
    private List<ProposedBooking> _proposedBookings = [];

    private bool _isProposalDialogOpen;
    private bool _isMonthReview;
    private readonly HashSet<ProposedBooking> _selectedProposals = [];

    private Booking? _editedBooking;
    private DateOnly? _editDate;
    private CategoryType _editType;
    private Guid? _editCategoryId;
    private string _editDescription = string.Empty;
    private decimal _editAmount;
    private BookingStatus _editStatus;
    private string? _amountError;
    private string? _categoryError;
    private bool _isSaving;

    [Inject]
    private IBookingManager BookingManager { get; set; } = default!;

    [Inject]
    private IHaushaltsbuchOverviewWorkflow OverviewWorkflow { get; set; } = default!;

    [Inject]
    private IRecurringBookingGenerationWorkflow RecurringBookingGenerationWorkflow { get; set; } = default!;

    [Inject]
    private IStatusMessageService StatusMessages { get; set; } = default!;

    [Inject]
    private TimeProvider TimeProvider { get; set; } = default!;

    private DateOnly FirstDay
        => new(_month.Year, _month.Month, 1);

    private DateOnly LastDay
        => new(_month.Year, _month.Month, DateTime.DaysInMonth(_month.Year, _month.Month));

    private string MonthLabel
        => $"{MonthNames[_month.Month - 1]} {_month.Year}";

    private decimal IncomeBooked
        => SumBookings(CategoryType.Income, BookingStatus.Booked);

    private decimal IncomePending
        => SumBookings(CategoryType.Income, BookingStatus.Pending);

    private decimal ExpenseBooked
        => SumBookings(CategoryType.Expense, BookingStatus.Booked);

    private decimal ExpensePending
        => SumBookings(CategoryType.Expense, BookingStatus.Pending);

    private decimal Balance
        => (IncomeBooked + IncomePending) - (ExpenseBooked + ExpensePending);

    private decimal BookedBalance
        => IncomeBooked - ExpenseBooked;

    private bool IsNewBooking
        => (_editedBooking is not null) && !_bookings.Any(booking => booking.Id == _editedBooking.Id);

    private string DialogTitle
        => IsNewBooking ? "Neue Buchung" : "Buchung bearbeiten";

    private string GenerateFixBookingsLabel
        => _proposedBookings.Count == 0
            ? "Fixbuchungen vollständig"
            : $"Fixbuchungen für {MonthNames[_month.Month - 1]} generieren ({_proposedBookings.Count})";

    private string ProposalDialogTitle
        => _isMonthReview ? $"Fixposten für {MonthLabel} prüfen" : $"Fixbuchungen für {MonthLabel} generieren";

    protected override async Task OnInitializedAsync()
    {
        DateOnly today = DateOnly.FromDateTime(TimeProvider.GetLocalNow().DateTime);
        _month = new DateOnly(today.Year, today.Month, 1);
        await LoadAsync();
    }

    private async Task ChangeMonthAsync(DateOnly month)
    {
        _month = month;
        await LoadAsync();
    }

    private decimal SumBookings(CategoryType type, BookingStatus status)
        => _bookings.Where(booking => (booking.Type == type) && (booking.Status == status)).Sum(booking => booking.Amount);

    // A booking generated from a recurring item without a description of its own is labeled as such.
    private static string GetDescription(Booking booking)
        => !string.IsNullOrWhiteSpace(booking.Description)
            ? booking.Description
            : booking.RecurringItemId is null ? "—" : "Fixbuchung";

    private static string GetAmountClass(CategoryType type)
        => type == CategoryType.Income ? "gwios-amount gwios-amount--success" : "gwios-amount gwios-amount--danger";

    private static string FormatAmount(decimal value)
        => value.ToString("C", GermanCulture);

    private static string FormatSignedAmount(decimal value)
        => (value >= 0 ? "+ " : "– ") + Math.Abs(value).ToString("C", GermanCulture);

    private void OpenNewBooking()
    {
        DateOnly today = DateOnly.FromDateTime(TimeProvider.GetLocalNow().DateTime);
        DateOnly defaultDate = (today >= FirstDay) && (today <= LastDay) ? today : FirstDay;
        _editedBooking = new Booking
        {
            Date = defaultDate, Type = CategoryType.Expense, CategoryId = Guid.Empty, Amount = 0,
            Status = BookingStatus.Booked
        };
        _editDate = defaultDate;
        _editType = CategoryType.Expense;
        _editCategoryId = null;
        _editDescription = string.Empty;
        _editAmount = 0;
        _editStatus = BookingStatus.Booked;
        _amountError = null;
        _categoryError = null;
    }

    private void OpenEditBooking(Booking booking)
    {
        _editedBooking = booking;
        _editDate = booking.Date;
        _editType = booking.Type;
        _editCategoryId = booking.CategoryId;
        _editDescription = booking.Description ?? string.Empty;
        _editAmount = booking.Amount;
        _editStatus = booking.Status;
        _amountError = null;
        _categoryError = null;
    }

    private void CloseDialog()
        => _editedBooking = null;

    private void SelectEditType(string item)
    {
        _editType = item == "Einnahme" ? CategoryType.Income : CategoryType.Expense;
        _editCategoryId = null;
    }

    private void SelectEditStatus(string item)
        => _editStatus = item == "Gebucht" ? BookingStatus.Booked : BookingStatus.Pending;

    private async Task SaveBookingAsync()
    {
        if ((_editedBooking is null) || _isSaving)
        {
            return;
        }

        _amountError = _editAmount <= 0 ? "Bitte einen Betrag größer 0 angeben." : null;
        _categoryError = _editCategoryId is null ? "Bitte eine Kategorie wählen." : null;
        if ((_amountError is not null) || (_categoryError is not null) || (_editCategoryId is not Guid categoryId))
        {
            return;
        }

        bool isNew = IsNewBooking;
        Booking booking = new()
        {
            Id = _editedBooking.Id,
            Date = _editDate ?? FirstDay,
            Type = _editType,
            CategoryId = categoryId,
            Description = string.IsNullOrWhiteSpace(_editDescription) ? null : _editDescription.Trim(),
            Amount = _editAmount,
            Status = _editStatus,
            RecurringItemId = _editedBooking.RecurringItemId
        };

        _isSaving = true;
        try
        {
            if (isNew)
            {
                await BookingManager.CreateBookingAsync(booking);
                StatusMessages.Publish(StatusLevel.Neutral, $"Buchung über {FormatAmount(booking.Amount)} erfasst");
            }
            else
            {
                await BookingManager.UpdateBookingAsync(booking);
                StatusMessages.Publish(StatusLevel.Success, $"Buchung über {FormatAmount(booking.Amount)} gespeichert");
            }
        }
        finally
        {
            _isSaving = false;
        }

        CloseDialog();
        await ReloadDataAsync();
    }

    private async Task DeleteBookingAsync(Booking booking)
    {
        await BookingManager.DeleteBookingAsync(booking.Id);
        StatusMessages.Publish(StatusLevel.Warning, "Buchung gelöscht");
        await ReloadDataAsync();
    }

    private void ToggleProposal(ProposedBooking proposal, bool isSelected)
    {
        if (isSelected)
        {
            _selectedProposals.Add(proposal);
        }
        else
        {
            _selectedProposals.Remove(proposal);
        }
    }

    private void OpenFixBookingSelection()
    {
        if (_proposedBookings.Count == 0)
        {
            return;
        }

        OpenProposalDialog();
        _isMonthReview = false;
    }

    private void OpenMonthReview()
    {
        OpenProposalDialog();
        _isMonthReview = true;
    }

    private void OpenProposalDialog()
    {
        _selectedProposals.Clear();
        _selectedProposals.UnionWith(_proposedBookings);
        _isProposalDialogOpen = true;
    }

    private void CloseProposalDialog()
        => _isProposalDialogOpen = false;

    private async Task ConfirmProposalsAsync()
    {
        int confirmedCount = _selectedProposals.Count;
        await RecurringBookingGenerationWorkflow.ConfirmAsync(_month, [.. _selectedProposals]);
        CloseProposalDialog();
        StatusMessages.Publish(StatusLevel.Success, $"{confirmedCount} Fixbuchungen für {MonthLabel} übernommen");
        await ReloadDataAsync();
    }

    private async Task LoadAsync()
    {
        await ReloadDataAsync();

        bool needsReview = await RecurringBookingGenerationWorkflow.NeedsReviewAsync(_month);
        if (needsReview && (_proposedBookings.Count > 0))
        {
            OpenMonthReview();
        }
        else
        {
            CloseProposalDialog();
            if (needsReview)
            {
                // Nothing to propose this month; mark it reviewed without bothering anyone with an empty popup.
                await RecurringBookingGenerationWorkflow.ConfirmAsync(_month, []);
            }
        }
    }

    private async Task ReloadDataAsync()
    {
        HaushaltsbuchOverviewData overview = await OverviewWorkflow.GetOverviewAsync(FirstDay, LastDay);
        _categories = overview.Categories;
        _bookings = overview.Bookings;
        _proposedBookings = await RecurringBookingGenerationWorkflow.GetProposedBookingsAsync(_month);
    }
}
