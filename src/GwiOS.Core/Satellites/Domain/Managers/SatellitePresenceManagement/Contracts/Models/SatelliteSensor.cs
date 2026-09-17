namespace GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;

/// <summary>
/// A sensor of a satellite, described generally, so GwiOS needs no knowledge of individual sensor types.
/// </summary>
public class SatelliteSensor
{
    /// <summary>
    /// The type of the sensor, such as <c>bme280</c>.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// The quantities the sensor measures, such as <c>temperature</c>, <c>humidity</c> and <c>pressure</c>.
    /// </summary>
    public required List<string> Quantities { get; init; }
}
