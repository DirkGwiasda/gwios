namespace GwiOS.Core.Satellites.Domain.Workflows.SatellitePing.Contracts.Models;

/// <summary>
/// A ping a satellite sent to GwiOS, as received.
/// </summary>
public class SatellitePingData
{
    /// <summary>
    /// The name the satellite pinged with.
    /// </summary>
    public required string SatelliteName { get; init; }

    /// <summary>
    /// The token the satellite pinged with.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// The address of the HTTP server of the satellite, such as <c>http://192.168.178.33</c>.
    /// </summary>
    public required string Endpoint { get; init; }

    /// <summary>
    /// The readable type name of the board, such as <c>ESP32 DevKit C</c>.
    /// </summary>
    public required string Board { get; init; }

    /// <summary>
    /// The firmware version of the satellite, such as <c>0.1.0</c>.
    /// </summary>
    public required string Firmware { get; init; }

    /// <summary>
    /// The WLAN signal strength of the satellite in dBm.
    /// </summary>
    public int WifiRssiDbm { get; init; }

    /// <summary>
    /// The sensors of the satellite. Empty if it reported none.
    /// </summary>
    public required List<SatellitePingSensor> Sensors { get; init; }
}
