using GwiOS.Core.Satellites.Domain.Workflows.MeasurementPolling.Contracts;
using GwiOS.WebUI.BackgroundServices;
using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using MeasurementPollingServiceUnderTest = GwiOS.WebUI.BackgroundServices.MeasurementPollingService;

namespace GwiOS.WebUI.Tests.BackgroundServices.MeasurementPollingService;

/// <summary>
/// Runs a <c>MeasurementPollingService</c> with an interval of 30 seconds on a clock the test moves forward, and a
/// polling workflow fake registered as scoped service.
/// </summary>
internal sealed class MeasurementPollingServiceProbe : IDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public MeasurementPollingServiceProbe()
    {
        ServiceCollection services = new();
        services.AddScoped<IMeasurementPollingWorkflow>(_ => Workflow);
        _serviceProvider = services.BuildServiceProvider();
        Service = new MeasurementPollingServiceUnderTest(
            _serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new MeasurementPollingOptions { IntervalSeconds = 30 }),
            TimeProvider,
            Logger);
    }

    /// <summary>
    /// The workflow the service runs.
    /// </summary>
    public MeasurementPollingWorkflowFake Workflow { get; } = new();

    /// <summary>
    /// The clock of the service, which only moves when the test advances it.
    /// </summary>
    public FakeTimeProvider TimeProvider { get; } = new();

    /// <summary>
    /// The logger of the service.
    /// </summary>
    public LoggerFake<MeasurementPollingServiceUnderTest> Logger { get; } = new();

    /// <summary>
    /// The service under test.
    /// </summary>
    public MeasurementPollingServiceUnderTest Service { get; }

    /// <summary>
    /// Waits up to five seconds until the workflow was run the given number of times.
    /// </summary>
    public async Task WaitForPollCountAsync(int pollCount)
    {
        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(5));
        while (Workflow.PollCount < pollCount)
        {
            await Task.Delay(10, timeout.Token);
        }
    }

    public void Dispose()
    {
        Service.Dispose();
        _serviceProvider.Dispose();
    }
}
