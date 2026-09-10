using System.Collections.Immutable;

namespace GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

/// <summary>
/// A single log message written by an application.
/// </summary>
/// <param name="AppName">Name of the application the entry belongs to.</param>
/// <param name="DataSource">
/// Identifies the code within the application that wrote the entry, usually the full name of the logging type.
/// </param>
/// <param name="Message">The logged message text.</param>
/// <param name="LogLevel">Severity of the entry.</param>
/// <param name="ContextData">Additional key/value data describing the entry; <c>null</c> means no data.</param>
public record LogEntry(
    string AppName,
    string DataSource,
    string Message,
    LogLevel LogLevel,
    IReadOnlyDictionary<string, string>? ContextData = null)
{
    /// <summary>
    /// Unique identifier of the entry. Defaults to a new time-ordered UUID (version 7).
    /// </summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>
    /// Point in time the entry was written. Defaults to the current UTC time at creation.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Additional key/value data describing the entry. Never <c>null</c>; empty if the entry carries no data.
    /// </summary>
    public IReadOnlyDictionary<string, string> ContextData { get; init; } =
        ContextData ?? ImmutableDictionary<string, string>.Empty;
}
