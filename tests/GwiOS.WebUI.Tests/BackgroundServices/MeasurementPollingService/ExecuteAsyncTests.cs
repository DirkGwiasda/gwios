namespace GwiOS.WebUI.Tests.BackgroundServices.MeasurementPollingService;

/// <summary>
/// Covers the loop of <c>MeasurementPollingService</c>, started through <c>StartAsync</c>.
/// </summary>
public sealed class ExecuteAsyncTests : IDisposable
{
    private readonly MeasurementPollingServiceProbe _probe = new();

    [Fact]
    public async Task PollsRightAwayAndThenOncePerInterval()
    {
        await _probe.Service.StartAsync(Xunit.TestContext.Current.CancellationToken);
        await _probe.WaitForPollCountAsync(1);

        _probe.TimeProvider.Advance(TimeSpan.FromSeconds(30));
        await _probe.WaitForPollCountAsync(2);
        _probe.TimeProvider.Advance(TimeSpan.FromSeconds(30));
        await _probe.WaitForPollCountAsync(3);

        await _probe.Service.StopAsync(Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(3, _probe.Workflow.PollCount);
    }

    [Fact]
    public async Task DoesNotPollBeforeTheIntervalHasPassed()
    {
        await _probe.Service.StartAsync(Xunit.TestContext.Current.CancellationToken);
        await _probe.WaitForPollCountAsync(1);

        _probe.TimeProvider.Advance(TimeSpan.FromSeconds(29));
        await Task.Delay(50, Xunit.TestContext.Current.CancellationToken);

        await _probe.Service.StopAsync(Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(1, _probe.Workflow.PollCount);
    }

    [Fact]
    public async Task KeepsPolling_WhenAPollFails()
    {
        _probe.Workflow.ExceptionToThrow = new InvalidOperationException("Database not reachable.");
        await _probe.Service.StartAsync(Xunit.TestContext.Current.CancellationToken);
        await _probe.WaitForPollCountAsync(1);

        _probe.TimeProvider.Advance(TimeSpan.FromSeconds(30));
        await _probe.WaitForPollCountAsync(2);

        await _probe.Service.StopAsync(Xunit.TestContext.Current.CancellationToken);
    }

    public void Dispose()
        => _probe.Dispose();
}
