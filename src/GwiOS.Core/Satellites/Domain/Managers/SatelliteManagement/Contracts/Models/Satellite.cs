namespace GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;

/// <summary>
/// A satellite registered in GwiOS: a small hardware module that reports measurements over the LAN.
/// </summary>
public class Satellite
{
    /// <summary>
    /// The name of the satellite, which also identifies it: 1 to 32 characters out of <c>a–z</c>, <c>0–9</c> and
    /// <c>-</c>. It is unique across all satellites and stays unchanged, because the satellite sends it with every
    /// ping.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// A readable name shown instead of <see cref="Name"/>, or <c>null</c> if the satellite has none.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Whether the satellite is locked. GwiOS rejects the pings of a locked satellite and does not fetch its
    /// measurements.
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// The token of the satellite, encrypted with ASP.NET Data Protection, or <c>null</c> if the satellite has no
    /// token, such as an unknown satellite that was blocked. The plain token is never stored.
    /// </summary>
    public string? ProtectedToken { get; set; }

    /// <summary>
    /// Point in time the satellite was created, in UTC. Defaults to the current UTC time at creation.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
