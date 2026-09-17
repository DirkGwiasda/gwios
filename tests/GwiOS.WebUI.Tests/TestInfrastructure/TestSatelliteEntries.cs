using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Workflows.SatelliteOverview.Contracts.Models;

namespace GwiOS.WebUI.Tests.TestInfrastructure;

/// <summary>
/// Creates overview entries and candidates as the BME280 satellites deliver them, relative to <see cref="Now"/>.
/// </summary>
public sealed class TestSatelliteEntries
{
    /// <summary>
    /// The current point in time of the tests, 2026-07-14 10:30 UTC like the clock fakes.
    /// </summary>
    public static readonly DateTime Now = new(2026, 7, 14, 10, 30, 0, DateTimeKind.Utc);

    /// <summary>
    /// Returns an entry of an online satellite that pinged the given seconds ago, with the given temperature history
    /// (oldest first, fetched every 30 seconds up to now), a humidity of 44.4 % and a pressure of 1014.24 hPa.
    /// </summary>
    public SatelliteOverviewEntry CreateOnline(
        string name,
        string? displayName = null,
        double[]? temperatures = null,
        int secondsSincePing = 4,
        int wifiRssiDbm = -52)
        => new()
        {
            Satellite = new Satellite { Name = name, DisplayName = displayName, ProtectedToken = "protected" },
            Presence = CreatePresence(name, secondsSincePing, wifiRssiDbm),
            IsOnline = true,
            Histories =
            [
                CreateHistory(name, "humidity", "%", [44.4]),
                CreateHistory(name, "pressure", "hPa", [1014.24]),
                CreateHistory(name, "temperature", "°C", temperatures ?? [22.0, 22.4])
            ]
        };

    /// <summary>
    /// Returns the entry of the given online entry as if its satellite had gone offline the given minutes ago.
    /// </summary>
    public SatelliteOverviewEntry CreateOffline(string name, int minutesSincePing = 6)
    {
        SatelliteOverviewEntry online = CreateOnline(name, secondsSincePing: minutesSincePing * 60);
        return new SatelliteOverviewEntry
        {
            Satellite = online.Satellite,
            Presence = online.Presence,
            IsOnline = false,
            Histories = online.Histories
        };
    }

    /// <summary>
    /// Returns a candidate that pinged first five minutes and last ten seconds ago.
    /// </summary>
    public SatelliteCandidate CreateCandidate(string name, bool hasTokenChanged = false)
        => new()
        {
            Name = name,
            Endpoint = "http://192.168.178.40",
            Board = "ESP32 DevKit C",
            Firmware = "0.1.0",
            WifiRssiDbm = -75,
            FirstPingAt = Now.AddMinutes(-5),
            LastPingAt = Now.AddSeconds(-10),
            HasTokenChanged = hasTokenChanged
        };

    /// <summary>
    /// Returns a presence of the satellite with the given name that pinged the given seconds ago.
    /// </summary>
    public SatellitePresence CreatePresence(string name, int secondsSincePing = 4, int wifiRssiDbm = -52)
        => new()
        {
            SatelliteName = name,
            Endpoint = "http://192.168.0.41",
            Board = "ESP32 DevKit C",
            Firmware = "0.1.0",
            WifiRssiDbm = wifiRssiDbm,
            Sensors = [],
            LastPingAt = Now.AddSeconds(-secondsSincePing)
        };

    private static MeasurementHistory CreateHistory(string name, string quantity, string unit, double[] values)
        => new()
        {
            SatelliteName = name,
            Sensor = "bme280",
            Quantity = quantity,
            Unit = unit,
            RecentSamples =
            [
                .. values.Select((value, index) =>
                    new MeasurementSample(value, Now.AddSeconds(-30 * (values.Length - 1 - index)), Now))
            ],
            LongTermSamples = []
        };
}
