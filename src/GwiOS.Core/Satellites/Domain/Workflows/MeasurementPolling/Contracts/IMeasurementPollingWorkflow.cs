namespace GwiOS.Core.Satellites.Domain.Workflows.MeasurementPolling.Contracts;

/// <summary>
/// Fetches the measurements of the satellites.
/// </summary>
public interface IMeasurementPollingWorkflow
{
    /// <summary>
    /// Fetches the latest measurement of every satellite that is not locked and online, and adds it to the histories.
    /// A satellite that cannot be fetched is logged and skipped; the others are fetched nevertheless.
    /// </summary>
    Task PollAllAsync(CancellationToken cancellationToken = default);
}
