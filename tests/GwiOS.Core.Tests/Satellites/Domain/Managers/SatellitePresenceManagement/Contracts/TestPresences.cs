using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts;

/// <summary>
/// Creates presences with plausible values, as the BME280 satellite reports them.
/// </summary>
public sealed class TestPresences
{
    /// <summary>
    /// Returns a new valid presence of the satellite with the given name.
    /// </summary>
    public SatellitePresence Create(string satelliteName = "arbeitszimmer")
        => new()
        {
            SatelliteName = satelliteName,
            Endpoint = "http://192.168.178.33",
            Board = "ESP32 DevKit C",
            Firmware = "0.1.0",
            WifiRssiDbm = -70,
            Sensors = [new SatelliteSensor { Type = "bme280", Quantities = ["temperature", "humidity", "pressure"] }]
        };
}
