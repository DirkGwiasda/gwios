using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts;

/// <summary>
/// Keeps the measurement histories of the satellites. Safe to use from several threads at once.
/// </summary>
public interface IMeasurementHistoryRepository
{
    /// <summary>
    /// Adds the given sample to the history of the given satellite, sensor and quantity, which is created if it does
    /// not exist yet, and sets its unit. The short history keeps the last 10 samples; the long history keeps every
    /// 10th sample, starting with the first, up to the last 100.
    /// </summary>
    void AddSample(string satelliteName, Measurement measurement, MeasurementSample sample);

    /// <summary>
    /// Returns copies of all histories of the satellite with the given name, ordered by sensor and quantity. Empty if
    /// the satellite has none.
    /// </summary>
    List<MeasurementHistory> GetBySatelliteName(string satelliteName);
}
