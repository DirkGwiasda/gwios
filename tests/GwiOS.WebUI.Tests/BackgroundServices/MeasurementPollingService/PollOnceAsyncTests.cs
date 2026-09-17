using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

namespace GwiOS.WebUI.Tests.BackgroundServices.MeasurementPollingService;

/// <summary>
/// Covers <c>MeasurementPollingService.PollOnceAsync</c>.
/// </summary>
public sealed class PollOnceAsyncTests : IDisposable
{
    private readonly MeasurementPollingServiceProbe _probe = new();

    [Fact]
    public async Task RunsThePollingWorkflowOnce()
    {
        await _probe.Service.PollOnceAsync(Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(1, _probe.Workflow.PollCount);
        Assert.Empty(_probe.Logger.Entries);
    }

    [Fact]
    public async Task LogsAnErrorInsteadOfThrowing_WhenThePollFails()
    {
        _probe.Workflow.ExceptionToThrow = new InvalidOperationException("Database not reachable.");

        await _probe.Service.PollOnceAsync(Xunit.TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Error, logEntry.LogLevel);
        Assert.Equal("Database not reachable.", logEntry.Message);
    }

    [Fact]
    public async Task LetsACancellationPass()
    {
        _probe.Workflow.ExceptionToThrow = new OperationCanceledException();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _probe.Service.PollOnceAsync(Xunit.TestContext.Current.CancellationToken));
    }

    public void Dispose()
        => _probe.Dispose();
}
