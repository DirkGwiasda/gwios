namespace GwiOS.Core.Satellites.Domain.Workflows.SatellitePing.Contracts.Models;

/// <summary>
/// A sensor as a satellite describes it in its ping.
/// </summary>
public class SatellitePingSensor
{
    /// <summary>
    /// The type of the sensor, such as <c>bme280</c>.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// The quantities the sensor measures, such as <c>temperature</c>.
    /// </summary>
    public required List<string> Quantities { get; init; }
}
