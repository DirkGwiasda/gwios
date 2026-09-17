using GwiOS.Core.Satellites.Domain.Workflows.SatelliteBlocking.Contracts;

namespace GwiOS.WebUI.Tests.TestInfrastructure;

/// <summary>
/// Stands in for <see cref="ISatelliteBlockingWorkflow"/>: records every blocked name, or throws
/// <see cref="ExceptionToThrow"/> if set.
/// </summary>
public sealed class SatelliteBlockingWorkflowFake : ISatelliteBlockingWorkflow
{
    private readonly List<string> _blockedNames = [];

    /// <summary>
    /// All blocked names, in order.
    /// </summary>
    public IReadOnlyList<string> BlockedNames
        => _blockedNames;

    /// <summary>
    /// Exception every blocking throws, or <c>null</c> for none.
    /// </summary>
    public Exception? ExceptionToThrow { get; set; }

    public Task BlockCandidateAsync(string candidateName, CancellationToken cancellationToken = default)
    {
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        _blockedNames.Add(candidateName);
        return Task.CompletedTask;
    }
}
