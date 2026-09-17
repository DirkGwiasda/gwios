namespace GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;

/// <summary>
/// The latest measurement of a satellite, as fetched from it.
/// </summary>
public class MeasurementReading
{
    /// <summary>
    /// The values of the measurement, one per sensor and quantity. Empty if the satellite reported none.
    /// </summary>
    public required List<Measurement> Measurements { get; init; }

    /// <summary>
    /// How many seconds before the fetch the satellite measured the values. Satellites have no reliable clock, so
    /// GwiOS derives the time of measurement from it.
    /// </summary>
    public int AgeSeconds { get; init; }
}
