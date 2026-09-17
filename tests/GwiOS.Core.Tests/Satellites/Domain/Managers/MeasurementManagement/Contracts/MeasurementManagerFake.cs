using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.MeasurementManagement.Contracts;

/// <summary>
/// Stands in for <see cref="IMeasurementManager"/> for the workflow tests. Every satellite answers with
/// <see cref="Reading"/> unless it is listed in <see cref="UnreachableSatelliteNames"/>; recorded readings and fetch
/// requests are kept.
/// </summary>
public sealed class MeasurementManagerFake : IMeasurementManager
{
    private readonly List<(string SatelliteName, string Endpoint, string Token)> _fetches = [];
    private readonly List<(string SatelliteName, MeasurementReading Reading)> _recordedReadings = [];

    /// <summary>
    /// The reading every reachable satellite answers with.
    /// </summary>
    public MeasurementReading Reading { get; set; } = new() { Measurements = [] };

    /// <summary>
    /// The names of the satellites that cannot be reached.
    /// </summary>
    public HashSet<string> UnreachableSatelliteNames { get; } = [];

    /// <summary>
    /// The histories returned per satellite name.
    /// </summary>
    public Dictionary<string, List<MeasurementHistory>> Histories { get; } = [];

    /// <summary>
    /// All fetch requests so far, oldest first.
    /// </summary>
    public IReadOnlyList<(string SatelliteName, string Endpoint, string Token)> Fetches
        => _fetches;

    /// <summary>
    /// All recorded readings so far, oldest first.
    /// </summary>
    public IReadOnlyList<(string SatelliteName, MeasurementReading Reading)> RecordedReadings
        => _recordedReadings;

    public Task<MeasurementReading> FetchReadingAsync(
        string satelliteName,
        string endpoint,
        string token,
        CancellationToken cancellationToken = default)
    {
        _fetches.Add((satelliteName, endpoint, token));
        return UnreachableSatelliteNames.Contains(satelliteName)
            ? throw new MeasurementFetchException(satelliteName, "The satellite is not reachable.")
            : Task.FromResult(Reading);
    }

    public void RecordReading(string satelliteName, MeasurementReading reading)
        => _recordedReadings.Add((satelliteName, reading));

    public List<MeasurementHistory> GetHistories(string satelliteName)
        => Histories.GetValueOrDefault(satelliteName) ?? [];
}
