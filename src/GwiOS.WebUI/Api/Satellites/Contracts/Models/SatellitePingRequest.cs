namespace GwiOS.WebUI.Api.Satellites.Contracts.Models;

/// <summary>
/// The JSON body of a ping, as a satellite sends it. Every property is <c>null</c> if the body lacks it.
/// </summary>
public class SatellitePingRequest
{
    /// <summary>
    /// The address of the HTTP server of the satellite, such as <c>http://192.168.178.33</c>.
    /// </summary>
    public string? Endpoint { get; init; }

    /// <summary>
    /// The readable type name of the board, such as <c>ESP32 DevKit C</c>.
    /// </summary>
    public string? Board { get; init; }

    /// <summary>
    /// The firmware version of the satellite, such as <c>0.1.0</c>.
    /// </summary>
    public string? Firmware { get; init; }

    /// <summary>
    /// The WLAN signal strength of the satellite in dBm.
    /// </summary>
    public int? WifiRssiDbm { get; init; }

    /// <summary>
    /// The sensors of the satellite.
    /// </summary>
    public List<SatellitePingSensorRequest>? Sensors { get; init; }
}
