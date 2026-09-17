using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.MeasurementPolling.Contracts;
using Microsoft.Extensions.Options;

namespace GwiOS.WebUI.BackgroundServices;

/// <summary>
/// Fetches the measurements of all satellites in the interval of <see cref="MeasurementPollingOptions"/>, starting
/// right away. It only keeps the time: the work is done by <see cref="IMeasurementPollingWorkflow"/>, so a general
/// system for scheduled tasks can replace this service later.
/// </summary>
internal sealed class MeasurementPollingService(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<MeasurementPollingOptions> options,
    TimeProvider timeProvider,
    ILogger<MeasurementPollingService> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(options.Value.IntervalSeconds);
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<MeasurementPollingService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(_interval, _timeProvider);
        do
        {
            await PollOnceAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    /// <summary>
    /// Fetches the measurements of all satellites once. A failure is logged and does not stop the service, so the
    /// next interval tries again.
    /// </summary>
    internal async Task PollOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            // The workflow and its managers are scoped, so every run gets a scope of its own.
            await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
            IMeasurementPollingWorkflow workflow =
                scope.ServiceProvider.GetRequiredService<IMeasurementPollingWorkflow>();
            await workflow.PollAllAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception);
        }
    }
}
