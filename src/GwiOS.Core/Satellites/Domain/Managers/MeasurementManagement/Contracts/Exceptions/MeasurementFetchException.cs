namespace GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when the measurements of a satellite cannot be fetched: the satellite is not reachable, answers with an
/// error or with a response that cannot be read.
/// </summary>
/// <param name="satelliteName">The name of the satellite whose measurements could not be fetched.</param>
/// <param name="reason">Why the fetch failed, without the token.</param>
/// <param name="innerException">The error that caused the failure, or <c>null</c> if there is none.</param>
public sealed class MeasurementFetchException(string satelliteName, string reason, Exception? innerException = null)
    : Exception($"The measurements of satellite '{satelliteName}' cannot be fetched: {reason}", innerException)
{
    /// <summary>
    /// The name of the satellite whose measurements could not be fetched.
    /// </summary>
    public string SatelliteName { get; } = satelliteName;
}
