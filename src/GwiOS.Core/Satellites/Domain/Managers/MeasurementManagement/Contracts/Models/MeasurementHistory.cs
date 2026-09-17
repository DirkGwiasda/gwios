namespace GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;

/// <summary>
/// The fetched values of one quantity of one sensor of a satellite. Kept in memory only, so it is empty after a
/// restart of GwiOS.
/// </summary>
public class MeasurementHistory
{
    /// <summary>
    /// The name of the satellite that measured the values.
    /// </summary>
    public required string SatelliteName { get; init; }

    /// <summary>
    /// The type of the sensor that measured the values, such as <c>bme280</c>.
    /// </summary>
    public required string Sensor { get; init; }

    /// <summary>
    /// The measured quantity, such as <c>temperature</c>.
    /// </summary>
    public required string Quantity { get; init; }

    /// <summary>
    /// The unit of the latest value, such as <c>°C</c>.
    /// </summary>
    public required string Unit { get; init; }

    /// <summary>
    /// The short history: the last 10 fetched values, oldest first.
    /// </summary>
    public required List<MeasurementSample> RecentSamples { get; init; }

    /// <summary>
    /// The long history: every 10th fetched value, starting with the first, at most the last 100, oldest first.
    /// </summary>
    public required List<MeasurementSample> LongTermSamples { get; init; }
}
