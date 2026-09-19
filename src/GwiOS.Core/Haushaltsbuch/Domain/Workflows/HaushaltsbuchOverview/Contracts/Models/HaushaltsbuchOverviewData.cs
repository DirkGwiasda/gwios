using GwiOS.Core.Haushaltsbuch.Domain.Managers.BookingManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.CategoryManagement.Contracts.Models;
using GwiOS.Core.Haushaltsbuch.Domain.Managers.RecurringItemManagement.Contracts.Models;

namespace GwiOS.Core.Haushaltsbuch.Domain.Workflows.HaushaltsbuchOverview.Contracts.Models;

/// <summary>
/// The raw data the Dashboard, Fixkosten, Kategorien and Berichte tabs of the Haushaltsbuch are built from.
/// Formatting it for display (category paths, colors, sums, charts) is the job of the WebUI, not this data.
/// </summary>
public class HaushaltsbuchOverviewData
{
    /// <summary>
    /// All stored categories, ordered by type and then by name.
    /// </summary>
    public required List<Category> Categories { get; set; }

    /// <summary>
    /// All stored recurring items, ordered by type and then by name.
    /// </summary>
    public required List<RecurringItem> RecurringItems { get; set; }

    /// <summary>
    /// The bookings in the requested date range, newest first. The range is a single month for the Dashboard and
    /// Kategorien tabs, and a wider, selectable range for the Berichte tab.
    /// </summary>
    public required List<Booking> Bookings { get; set; }
}
