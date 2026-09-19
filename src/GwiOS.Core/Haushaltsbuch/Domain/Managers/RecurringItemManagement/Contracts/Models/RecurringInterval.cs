namespace GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;

/// <summary>
/// How often a recurring item in the Haushaltsbuch falls due.
/// </summary>
public enum RecurringInterval
{
    /// <summary>
    /// Falls due every week.
    /// </summary>
    Weekly,

    /// <summary>
    /// Falls due every month.
    /// </summary>
    Monthly,

    /// <summary>
    /// Falls due every three months.
    /// </summary>
    Quarterly,

    /// <summary>
    /// Falls due once a year.
    /// </summary>
    Yearly
}
