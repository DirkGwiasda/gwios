using System.Globalization;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.HaushaltsbuchOverview.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.HaushaltsbuchOverview.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.Pages.Haushaltsbuch;

/// <summary>
/// Fixkosten tab of the Haushaltsbuch: the recurring income and expense items, created, changed in a dialog,
/// activated/deactivated and deleted.
/// </summary>
public partial class Fixkosten
{
    private static readonly string[] TabItems = ["Wiederkehrende Ausgaben", "Wiederkehrende Einnahmen"];
    private static readonly string[] MonthNames =
    [
        "Januar", "Februar", "März", "April", "Mai", "Juni",
        "Juli", "August", "September", "Oktober", "November", "Dezember"
    ];
    private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-DE");

    private List<RecurringItem> _recurringItems = [];
    private List<Category> _categories = [];
    private CategoryType _selectedType = CategoryType.Expense;

    private RecurringItem? _editedItem;
    private string _editName = string.Empty;
    private Guid? _editCategoryId;
    private CategoryType _editType;
    private decimal _editAmount;
    private RecurringInterval _editInterval = RecurringInterval.Monthly;
    private int _editDueDay = 1;
    private int _editStartMonth;
    private bool _editActive = true;
    private string? _nameError;
    private string? _amountError;
    private bool _isSaving;

    [Inject]
    private IRecurringItemManager RecurringItemManager { get; set; } = default!;

    [Inject]
    private IHaushaltsbuchOverviewWorkflow OverviewWorkflow { get; set; } = default!;

    [Inject]
    private IStatusMessageService StatusMessages { get; set; } = default!;

    [Inject]
    private TimeProvider TimeProvider { get; set; } = default!;

    private string SelectedTabItem
        => _selectedType == CategoryType.Income ? "Wiederkehrende Einnahmen" : "Wiederkehrende Ausgaben";

    private List<RecurringItem> FilteredItems
        => [.. _recurringItems.Where(item => item.Type == _selectedType)];

    private decimal ActiveMonthlyEquivalentSum
        => FilteredItems.Where(item => item.Active).Sum(GetMonthlyEquivalent);

    private bool IsNewItem
        => (_editedItem is not null) && !_recurringItems.Any(item => item.Id == _editedItem.Id);

    private string DialogTitle
        => IsNewItem ? "Neuer wiederkehrender Posten" : "Wiederkehrenden Posten bearbeiten";

    protected override async Task OnInitializedAsync()
        => await LoadAsync();

    private void SelectTab(string item)
        => _selectedType = item == "Wiederkehrende Einnahmen" ? CategoryType.Income : CategoryType.Expense;

    private void SelectEditType(string item)
    {
        _editType = item == "Fixeinnahme" ? CategoryType.Income : CategoryType.Expense;
        _editCategoryId = null;
    }

    private void SelectEditActive(string item)
        => _editActive = item == "Aktiv";

    private void OpenNewItem()
    {
        _editedItem = new RecurringItem
        {
            Name = string.Empty, Type = _selectedType, CategoryId = Guid.Empty, Amount = 0,
            Interval = RecurringInterval.Monthly, DueDay = 1, StartMonth = 0, Active = true
        };
        _editName = string.Empty;
        _editCategoryId = null;
        _editType = _selectedType;
        _editAmount = 0;
        _editInterval = RecurringInterval.Monthly;
        _editDueDay = 1;
        _editStartMonth = 0;
        _editActive = true;
        _nameError = null;
        _amountError = null;
    }

    private void OpenEditItem(RecurringItem item)
    {
        _editedItem = item;
        _editName = item.Name;
        _editCategoryId = item.CategoryId;
        _editType = item.Type;
        _editAmount = item.Amount;
        _editInterval = item.Interval;
        _editDueDay = item.DueDay;
        _editStartMonth = item.StartMonth;
        _editActive = item.Active;
        _nameError = null;
        _amountError = null;
    }

    private void CloseDialog()
        => _editedItem = null;

    private async Task SaveItemAsync()
    {
        if ((_editedItem is null) || _isSaving)
        {
            return;
        }

        _nameError = string.IsNullOrWhiteSpace(_editName) ? "Die Bezeichnung ist ein Pflichtfeld." : null;
        _amountError = _editAmount <= 0 ? "Bitte einen Betrag größer 0 angeben." : null;
        if ((_nameError is not null) || (_amountError is not null))
        {
            return;
        }

        bool isNew = IsNewItem;
        RecurringItem item = new()
        {
            Id = _editedItem.Id,
            Name = _editName.Trim(),
            Type = _editType,
            CategoryId = _editCategoryId ?? Guid.Empty,
            Amount = _editAmount,
            Interval = _editInterval,
            DueDay = Math.Clamp(_editDueDay, 1, 28),
            StartMonth = _editStartMonth,
            Active = _editActive
        };

        _isSaving = true;
        try
        {
            if (isNew)
            {
                await RecurringItemManager.CreateRecurringItemAsync(item);
                StatusMessages.Publish(StatusLevel.Neutral, $"Posten „{item.Name}“ angelegt");
            }
            else
            {
                await RecurringItemManager.UpdateRecurringItemAsync(item);
                StatusMessages.Publish(StatusLevel.Success, $"Posten „{item.Name}“ gespeichert");
            }
        }
        catch (RecurringItemValidationException)
        {
            StatusMessages.Publish(StatusLevel.Error, "Posten konnte nicht gespeichert werden – Eingaben prüfen");
            return;
        }
        finally
        {
            _isSaving = false;
        }

        CloseDialog();
        await LoadAsync();
    }

    private async Task ToggleActiveAsync(RecurringItem item)
    {
        RecurringItem changed = new()
        {
            Id = item.Id, Name = item.Name, Type = item.Type, CategoryId = item.CategoryId, Amount = item.Amount,
            Interval = item.Interval, DueDay = item.DueDay, StartMonth = item.StartMonth, Active = !item.Active
        };
        await RecurringItemManager.UpdateRecurringItemAsync(changed);
        StatusMessages.Publish(
            changed.Active ? StatusLevel.Success : StatusLevel.Warning,
            $"Posten „{item.Name}“ {(changed.Active ? "aktiviert" : "deaktiviert")}");
        await LoadAsync();
    }

    private async Task DeleteItemAsync(RecurringItem item)
    {
        await RecurringItemManager.DeleteRecurringItemAsync(item.Id);
        StatusMessages.Publish(StatusLevel.Warning, $"Posten „{item.Name}“ gelöscht");
        await LoadAsync();
    }

    private decimal GetMonthlyEquivalent(RecurringItem item)
        => item.Interval switch
        {
            RecurringInterval.Weekly => item.Amount * 52 / 12,
            RecurringInterval.Monthly => item.Amount,
            RecurringInterval.Quarterly => item.Amount / 3,
            RecurringInterval.Yearly => item.Amount / 12,
            _ => item.Amount
        };

    private static string GetIntervalLabel(RecurringInterval interval)
        => interval switch
        {
            RecurringInterval.Weekly => "Wöchentlich",
            RecurringInterval.Monthly => "Monatlich",
            RecurringInterval.Quarterly => "Quartalsweise",
            RecurringInterval.Yearly => "Jährlich",
            _ => interval.ToString()
        };

    private static string GetDueLabel(RecurringItem item)
        => item.Interval switch
        {
            RecurringInterval.Weekly => "Wöchentlich, 4× im Monat",
            RecurringInterval.Monthly => $"Jeweils zum {item.DueDay}.",
            RecurringInterval.Quarterly => $"Quartalsweise zum {item.DueDay}.",
            RecurringInterval.Yearly => $"Jährlich im {MonthNames[item.StartMonth]}, zum {item.DueDay}.",
            _ => string.Empty
        };

    private string GetCategoryPath(Guid categoryId)
    {
        Dictionary<Guid, Category> categoriesById = _categories.ToDictionary(category => category.Id);
        List<string> parts = [];
        Guid? currentId = categoryId;
        int guard = 0;
        while ((currentId is Guid id) && categoriesById.TryGetValue(id, out Category? category) && (guard++ < 8))
        {
            parts.Insert(0, category.Name);
            currentId = category.ParentId;
        }

        return parts.Count == 0 ? "—" : string.Join(" › ", parts);
    }

    private static string GetAmountClass(CategoryType type)
        => type == CategoryType.Income ? "gwios-amount gwios-amount--success" : "gwios-amount gwios-amount--danger";

    private static string FormatAmount(decimal value)
        => value.ToString("C", GermanCulture);

    // The Fixkosten tab does not need any bookings, so it asks the overview workflow for the smallest possible range.
    private async Task LoadAsync()
    {
        DateOnly today = DateOnly.FromDateTime(TimeProvider.GetLocalNow().DateTime);
        HaushaltsbuchOverviewData overview = await OverviewWorkflow.GetOverviewAsync(today, today);
        _recurringItems = overview.RecurringItems;
        _categories = overview.Categories;
    }
}
