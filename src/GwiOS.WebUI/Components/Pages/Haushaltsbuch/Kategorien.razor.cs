using System.Globalization;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Exceptions;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.CategoryDeletion.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.HaushaltsbuchOverview.Contracts;
using GwiOS.Core.Haushaltsbuch.Domain.Workflows.HaushaltsbuchOverview.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using Microsoft.AspNetCore.Components;

namespace GwiOS.WebUI.Components.Pages.Haushaltsbuch;

/// <summary>
/// Kategorien tab of the Haushaltsbuch: two trees (income and expense) where categories are created, changed in a
/// dialog and deleted, each showing its booked sum for the current month.
/// </summary>
public partial class Kategorien
{
    private static readonly string[] ColorSwatches =
        ["#00ffa3", "#3399ff", "#ffd056", "#ff4d6d", "#a093f2", "#ff8a5c", "#35d6c0"];
    private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-DE");

    private List<Category> _categories = [];
    private Dictionary<Guid, decimal> _sums = [];
    private readonly HashSet<Guid> _collapsedCategoryIds = [];

    private Category? _editedCategory;
    private string _editName = string.Empty;
    private Guid? _editParentId;
    private CategoryType _editType;
    private string _editColor = ColorSwatches[0];
    private string? _nameError;
    private bool _isSaving;

    [Inject]
    private ICategoryManager CategoryManager { get; set; } = default!;

    [Inject]
    private ICategoryDeletionWorkflow CategoryDeletionWorkflow { get; set; } = default!;

    [Inject]
    private IHaushaltsbuchOverviewWorkflow OverviewWorkflow { get; set; } = default!;

    [Inject]
    private IStatusMessageService StatusMessages { get; set; } = default!;

    [Inject]
    private TimeProvider TimeProvider { get; set; } = default!;

    private List<Category> IncomeCategories
        => [.. _categories.Where(category => category.Type == CategoryType.Income)];

    private List<Category> ExpenseCategories
        => [.. _categories.Where(category => category.Type == CategoryType.Expense)];

    private bool IsNewCategory
        => (_editedCategory is not null) && !_categories.Any(category => category.Id == _editedCategory.Id);

    private string DialogTitle
        => IsNewCategory ? "Neue Kategorie" : "Kategorie bearbeiten";

    protected override async Task OnInitializedAsync()
        => await LoadAsync();

    private decimal GetTypeSum(CategoryType type)
        => _categories.Where(category => category.Type == type).Sum(category => _sums.GetValueOrDefault(category.Id));

    private string FormatSum(decimal value)
        => value == 0 ? "—" : value.ToString("C", GermanCulture);

    private void ToggleCollapse(Guid categoryId)
    {
        if (!_collapsedCategoryIds.Remove(categoryId))
        {
            _collapsedCategoryIds.Add(categoryId);
        }
    }

    private void OpenNewCategory(CategoryType type, Guid? parentId)
    {
        _editedCategory = new Category { Name = string.Empty, Type = type };
        _editName = string.Empty;
        _editParentId = parentId;
        _editType = type;
        _editColor = ColorSwatches[Random.Shared.Next(ColorSwatches.Length)];
        _nameError = null;
    }

    private void OpenEditCategory(Category category)
    {
        _editedCategory = category;
        _editName = category.Name;
        _editParentId = category.ParentId;
        _editType = category.Type;
        _editColor = category.Color ?? ColorSwatches[0];
        _nameError = null;
    }

    private void CloseDialog()
        => _editedCategory = null;

    private async Task SaveCategoryAsync()
    {
        if ((_editedCategory is null) || _isSaving)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_editName))
        {
            _nameError = "Der Name ist ein Pflichtfeld.";
            return;
        }

        bool isNew = IsNewCategory;
        Category category = new()
        {
            Id = _editedCategory.Id,
            Name = _editName.Trim(),
            Type = _editType,
            ParentId = _editParentId,
            Color = _editParentId is null ? _editColor : null
        };

        _isSaving = true;
        try
        {
            if (isNew)
            {
                await CategoryManager.CreateCategoryAsync(category);
                StatusMessages.Publish(StatusLevel.Neutral, $"Kategorie „{category.Name}“ angelegt");
            }
            else
            {
                await CategoryManager.UpdateCategoryAsync(category);
                StatusMessages.Publish(StatusLevel.Success, $"Kategorie „{category.Name}“ gespeichert");
            }
        }
        catch (CategoryValidationException)
        {
            StatusMessages.Publish(StatusLevel.Error, "Kategorie konnte nicht gespeichert werden – Eingaben prüfen");
            return;
        }
        finally
        {
            _isSaving = false;
        }

        CloseDialog();
        await LoadAsync();
    }

    private async Task DeleteCategoryAsync(Category category)
    {
        try
        {
            await CategoryDeletionWorkflow.DeleteCategoryAsync(category.Id);
        }
        catch (CategoryHasChildrenException)
        {
            StatusMessages.Publish(
                StatusLevel.Error,
                $"Kategorie „{category.Name}“ hat Unterkategorien und kann nicht gelöscht werden");
            return;
        }
        catch (CategoryInUseException)
        {
            StatusMessages.Publish(
                StatusLevel.Error,
                $"Kategorie „{category.Name}“ wird noch verwendet und kann nicht gelöscht werden");
            return;
        }

        StatusMessages.Publish(StatusLevel.Warning, $"Kategorie „{category.Name}“ gelöscht");
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        DateOnly today = DateOnly.FromDateTime(TimeProvider.GetLocalNow().DateTime);
        DateOnly firstDay = new(today.Year, today.Month, 1);
        DateOnly lastDay = new(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
        HaushaltsbuchOverviewData overview = await OverviewWorkflow.GetOverviewAsync(firstDay, lastDay);
        _categories = overview.Categories;
        _sums = ComputeSums(_categories, overview.Bookings);
    }

    // A category's shown sum covers all its descendants too, not only its own direct bookings.
    private static Dictionary<Guid, decimal> ComputeSums(List<Category> categories, List<Booking> bookings)
    {
        Dictionary<Guid, decimal> sums = [];
        foreach (Category category in categories)
        {
            HashSet<Guid> descendantIds = CollectWithDescendants(category.Id, categories);
            sums[category.Id] = bookings
                .Where(booking => descendantIds.Contains(booking.CategoryId))
                .Sum(booking => booking.Amount);
        }

        return sums;
    }

    private static HashSet<Guid> CollectWithDescendants(Guid categoryId, List<Category> categories)
    {
        HashSet<Guid> result = [categoryId];
        List<Guid> pending = [categoryId];
        int index = 0;
        while (index < pending.Count)
        {
            Guid currentId = pending[index++];
            foreach (Category child in categories.Where(category => category.ParentId == currentId))
            {
                if (result.Add(child.Id))
                {
                    pending.Add(child.Id);
                }
            }
        }

        return result;
    }
}
