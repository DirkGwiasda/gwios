namespace GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;

/// <summary>
/// What a registered satellite reported with its latest accepted ping. Kept in memory only, so it is unknown after a
/// restart of GwiOS until the satellite pings again.
/// </summary>
public class SatellitePresence
{
    /// <summary>
    /// The name of the satellite the ping came from.
    /// </summary>
    public required string SatelliteName { get; init; }

    /// <summary>
    /// The address of the HTTP server of the satellite, such as <c>http://192.168.178.33</c>; GwiOS fetches the
    /// measurements from there.
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
    /// The WLAN signal strength of the satellite in dBm at the time of the ping; closer to 0 is stronger.
    /// </summary>
    public int WifiRssiDbm { get; init; }

    /// <summary>
    /// The sensors of the satellite. Empty if it reported none.
    /// </summary>
    public required List<SatelliteSensor> Sensors { get; init; }

    /// <summary>
    /// Point in time GwiOS received the ping, in UTC. Set when the ping is recorded.
    /// </summary>
    public DateTime LastPingAt { get; set; }
}
