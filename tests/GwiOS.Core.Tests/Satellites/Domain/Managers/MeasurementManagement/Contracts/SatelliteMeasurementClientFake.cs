using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.MeasurementManagement.Contracts;

/// <summary>
/// Stands in for <see cref="ISatelliteMeasurementClient"/>: returns <see cref="Reading"/> for every request, or fails
/// with a <see cref="MeasurementFetchException"/> while <see cref="IsReachable"/> is cleared, and records the requests.
/// </summary>
public sealed class SatelliteMeasurementClientFake : ISatelliteMeasurementClient
{
    private readonly List<(string SatelliteName, string Endpoint, string Token)> _requests = [];

    /// <summary>
    /// The reading returned for every request.
    /// </summary>
    public MeasurementReading Reading { get; set; } = new() { Measurements = [], AgeSeconds = 0 };

    /// <summary>
    /// Whether the satellites answer. <c>true</c> by default.
    /// </summary>
    public bool IsReachable { get; set; } = true;

    /// <summary>
    /// All requests so far, oldest first.
    /// </summary>
    public IReadOnlyList<(string SatelliteName, string Endpoint, string Token)> Requests
        => _requests;

    public Task<MeasurementReading> GetReadingAsync(
        string satelliteName,
        string endpoint,
        string token,
        CancellationToken cancellationToken = default)
    {
        _requests.Add((satelliteName, endpoint, token));
        return IsReachable
            ? Task.FromResult(Reading)
            : throw new MeasurementFetchException(satelliteName, "The satellite is not reachable.");
    }
}
