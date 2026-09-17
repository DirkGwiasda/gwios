namespace GwiOS.WebUI.Api.Satellites.Contracts.Models;

/// <summary>
/// A sensor in the JSON body of a ping. Every property is <c>null</c> if the body lacks it.
/// </summary>
public class SatellitePingSensorRequest
{
    /// <summary>
    /// The type of the sensor, such as <c>bme280</c>.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// The quantities the sensor measures, such as <c>temperature</c>.
    /// </summary>
    public List<string>? Quantities { get; init; }
}
