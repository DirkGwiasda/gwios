using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts;

/// <summary>
/// Fetches the latest measurement from the HTTP server of a satellite.
/// </summary>
public interface ISatelliteMeasurementClient
{
    /// <summary>
    /// Requests <c>GET /measurements</c> from the given endpoint with the given token and returns the reading. Throws a
    /// <see cref="MeasurementFetchException"/> if the satellite is not reachable in time, answers with another status
    /// than <c>200</c> or with a response that cannot be read.
    /// </summary>
    Task<MeasurementReading> GetReadingAsync(
        string satelliteName,
        string endpoint,
        string token,
        CancellationToken cancellationToken = default);
}
