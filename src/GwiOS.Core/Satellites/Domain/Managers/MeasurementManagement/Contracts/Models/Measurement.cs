namespace GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;

/// <summary>
/// A single value a satellite measured with one of its sensors.
/// </summary>
public class Measurement
{
    /// <summary>
    /// The type of the sensor that measured the value, such as <c>bme280</c>.
    /// </summary>
    public required string Sensor { get; init; }

    /// <summary>
    /// The measured quantity, such as <c>temperature</c>.
    /// </summary>
    public required string Quantity { get; init; }

    /// <summary>
    /// The measured value in <see cref="Unit"/>.
    /// </summary>
    public double Value { get; init; }

    /// <summary>
    /// The unit of the value, such as <c>°C</c>, <c>%</c> or <c>hPa</c>.
    /// </summary>
    public required string Unit { get; init; }
}
