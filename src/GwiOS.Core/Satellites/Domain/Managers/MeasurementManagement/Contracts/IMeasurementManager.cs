using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts;

/// <summary>
/// Manages the measurements of the satellites: fetches them from the satellites and keeps their histories.
/// </summary>
public interface IMeasurementManager
{
    /// <summary>
    /// Fetches the latest measurement from the satellite with the given name at the given endpoint, authenticated with
    /// the given token. Throws a <see cref="MeasurementFetchException"/> if the measurement cannot be fetched.
    /// </summary>
    Task<MeasurementReading> FetchReadingAsync(
        string satelliteName,
        string endpoint,
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the given reading of the satellite with the given name and adds its values to the histories, stamped
    /// with the current time as fetch time and the time of measurement derived from
    /// <see cref="MeasurementReading.AgeSeconds"/>. Throws a <see cref="MeasurementValidationException"/> if the
    /// reading is invalid; then no value is added.
    /// </summary>
    void RecordReading(string satelliteName, MeasurementReading reading);

    /// <summary>
    /// Returns all histories of the satellite with the given name, ordered by sensor and quantity. Empty if no values
    /// were fetched from it since GwiOS started.
    /// </summary>
    List<MeasurementHistory> GetHistories(string satelliteName);
}
