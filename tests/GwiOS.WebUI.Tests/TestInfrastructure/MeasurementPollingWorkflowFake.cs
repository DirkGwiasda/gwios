using GwiOS.Core.Satellites.Domain.Workflows.MeasurementPolling.Contracts;

namespace GwiOS.WebUI.Tests.TestInfrastructure;

/// <summary>
/// Stands in for <see cref="IMeasurementPollingWorkflow"/>: counts the polls and waits for <see cref="PollReleased"/>
/// before it finishes, so a test can observe a page while it polls. Throws <see cref="ExceptionToThrow"/> if set.
/// </summary>
public sealed class MeasurementPollingWorkflowFake : IMeasurementPollingWorkflow
{
    private int _pollCount;

    /// <summary>
    /// How often the measurements were polled.
    /// </summary>
    public int PollCount
        => _pollCount;

    /// <summary>
    /// Task every poll waits for; already completed unless a test holds polls back.
    /// </summary>
    public Task PollReleased { get; set; } = Task.CompletedTask;

    /// <summary>
    /// Exception every poll throws, or <c>null</c> for none.
    /// </summary>
    public Exception? ExceptionToThrow { get; set; }

    public async Task PollAllAsync(CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _pollCount);
        await PollReleased;
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }
    }
}
