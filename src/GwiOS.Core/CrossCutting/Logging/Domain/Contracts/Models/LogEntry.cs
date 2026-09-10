using System.Collections.Immutable;

namespace GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

public record LogEntry(
    string AppName,
    string DataSource,
    string Message,
    LogLevel LogLevel,
    IReadOnlyDictionary<string, string>? ContextData = null)
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    public IReadOnlyDictionary<string, string> ContextData { get; init; } =
        ContextData ?? ImmutableDictionary<string, string>.Empty;
}