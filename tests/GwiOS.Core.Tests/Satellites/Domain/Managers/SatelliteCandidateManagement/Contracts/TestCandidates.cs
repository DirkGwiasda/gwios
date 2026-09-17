using GwiOS.Core.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteCandidateManagement.Contracts;

/// <summary>
/// Creates candidates with plausible values and provides valid tokens.
/// </summary>
public sealed class TestCandidates
{
    /// <summary>
    /// A token in the format satellites accept.
    /// </summary>
    public const string Token = "abcdefghijklmnopqrstuvwxyzABCDEF0123456789_";

    /// <summary>
    /// Another token in the format satellites accept.
    /// </summary>
    public const string OtherToken = "ZYXWVUTSRQPONMLKJIHGFEDCBAzyxwvutsrqponmlkj";

    /// <summary>
    /// Returns a new candidate with the given name, as recorded from a ping.
    /// </summary>
    public SatelliteCandidate Create(string name = "garage", int wifiRssiDbm = -75)
        => new()
        {
            Name = name,
            Endpoint = "http://192.168.178.40",
            Board = "ESP32 DevKit C",
            Firmware = "0.1.0",
            WifiRssiDbm = wifiRssiDbm
        };
}
