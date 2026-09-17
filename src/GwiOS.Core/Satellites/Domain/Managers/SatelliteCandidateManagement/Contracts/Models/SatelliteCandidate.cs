namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;

/// <summary>
/// A satellite that pings GwiOS with a name no registered satellite has, and that can be adopted or blocked. Kept in
/// memory only. Its token is kept apart and is not part of this model, so it cannot be shown by accident.
/// </summary>
public class SatelliteCandidate
{
    /// <summary>
    /// The name the satellite pings with.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The address of the HTTP server the satellite reported, such as <c>http://192.168.178.33</c>.
    /// </summary>
    public required string Endpoint { get; init; }

    /// <summary>
    /// The readable type name of the board the satellite reported, such as <c>ESP32 DevKit C</c>.
    /// </summary>
    public required string Board { get; init; }

    /// <summary>
    /// The firmware version the satellite reported, such as <c>0.1.0</c>.
    /// </summary>
    public required string Firmware { get; init; }

    /// <summary>
    /// The WLAN signal strength in dBm the satellite reported with its latest ping.
    /// </summary>
    public int WifiRssiDbm { get; init; }

    /// <summary>
    /// Point in time of the first ping with this name, in UTC. Set when the ping is recorded.
    /// </summary>
    public DateTime FirstPingAt { get; set; }

    /// <summary>
    /// Point in time of the latest ping with this name, in UTC. Set when the ping is recorded.
    /// </summary>
    public DateTime LastPingAt { get; set; }

    /// <summary>
    /// Whether pings with this name came with different tokens. Then two senders use the name, and the candidate
    /// cannot be adopted, because the wrong sender could be registered. Set when the ping is recorded.
    /// </summary>
    public bool HasTokenChanged { get; set; }
}
