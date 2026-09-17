using GwiOS.Core.Satellites.Domain.Workflows.SatelliteAdoption.Contracts;

namespace GwiOS.WebUI.Tests.TestInfrastructure;

/// <summary>
/// Stands in for <see cref="ISatelliteAdoptionWorkflow"/>: records every adoption, or throws
/// <see cref="ExceptionToThrow"/> if set.
/// </summary>
public sealed class SatelliteAdoptionWorkflowFake : ISatelliteAdoptionWorkflow
{
    private readonly List<(string CandidateName, string? DisplayName)> _adoptions = [];

    /// <summary>
    /// All adoptions, in order.
    /// </summary>
    public IReadOnlyList<(string CandidateName, string? DisplayName)> Adoptions
        => _adoptions;

    /// <summary>
    /// Exception every adoption throws, or <c>null</c> for none.
    /// </summary>
    public Exception? ExceptionToThrow { get; set; }

    public Task AdoptCandidateAsync(string candidateName, string? displayName, CancellationToken cancellationToken = default)
    {
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        _adoptions.Add((candidateName, displayName));
        return Task.CompletedTask;
    }
}
