namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;

/// <summary>
/// Whether a Haushaltsbuch category, booking or recurring item represents income or an expense.
/// </summary>
public enum CategoryType
{
    /// <summary>
    /// Money coming in.
    /// </summary>
    Income,

    /// <summary>
    /// Money going out.
    /// </summary>
    Expense
}
