using GwiOS.Core.Satellites.Domain.Workflows.SatellitePing.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.SatellitePing.Contracts.Models;

namespace GwiOS.WebUI.Tests.TestInfrastructure;

/// <summary>
/// Stands in for <see cref="ISatellitePingWorkflow"/>: records every ping, then throws <see cref="ExceptionToThrow"/> if
/// set.
/// </summary>
public sealed class SatellitePingWorkflowFake : ISatellitePingWorkflow
{
    private readonly List<SatellitePingData> _pings = [];

    /// <summary>
    /// All processed pings, in order.
    /// </summary>
    public IReadOnlyList<SatellitePingData> Pings
        => _pings;

    /// <summary>
    /// Exception every ping throws, or <c>null</c> for none.
    /// </summary>
    public Exception? ExceptionToThrow { get; set; }

    public Task ProcessPingAsync(SatellitePingData ping, CancellationToken cancellationToken = default)
    {
        _pings.Add(ping);
        return ExceptionToThrow is null ? Task.CompletedTask : throw ExceptionToThrow;
    }
}
