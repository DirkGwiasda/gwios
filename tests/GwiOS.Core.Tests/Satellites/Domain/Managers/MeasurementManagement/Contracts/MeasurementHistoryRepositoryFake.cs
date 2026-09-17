using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.MeasurementManagement.Contracts;

/// <summary>
/// Stands in for <see cref="IMeasurementHistoryRepository"/> by recording every added sample, and returns
/// <see cref="Histories"/> for every satellite.
/// </summary>
public sealed class MeasurementHistoryRepositoryFake : IMeasurementHistoryRepository
{
    private readonly List<(string SatelliteName, Measurement Measurement, MeasurementSample Sample)> _addedSamples = [];

    /// <summary>
    /// All added samples, oldest first.
    /// </summary>
    public IReadOnlyList<(string SatelliteName, Measurement Measurement, MeasurementSample Sample)> AddedSamples
        => _addedSamples;

    /// <summary>
    /// The histories returned for every satellite. Empty by default.
    /// </summary>
    public List<MeasurementHistory> Histories { get; set; } = [];

    public void AddSample(string satelliteName, Measurement measurement, MeasurementSample sample)
        => _addedSamples.Add((satelliteName, measurement, sample));

    public List<MeasurementHistory> GetBySatelliteName(string satelliteName)
        => [.. Histories.Where(history => history.SatelliteName == satelliteName)];
}
