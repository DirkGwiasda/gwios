namespace GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

/// <summary>
/// One page of stored log entries, together with the number of stored entries on all pages.
/// </summary>
/// <param name="Entries">
/// The entries on the page, newest first. Holds at most the requested page size; empty if the page lies beyond the
/// last entry.
/// </param>
/// <param name="TotalCount">
/// The number of stored entries across all pages that match the request, not only those on this page. Zero if no
/// entries match.
/// </param>
public record LogEntryPage(IReadOnlyList<LogEntry> Entries, int TotalCount);
