using Microsoft.AspNetCore.Http;

namespace GwiOS.WebUI.Api.Satellites.Contracts;

/// <summary>
/// Handles <c>POST /api/satellites/{name}/ping</c>. Satellites authenticate with their token as bearer token instead
/// of the sign-in of users, so the endpoint answers with status codes itself instead of redirecting to the sign-in.
/// </summary>
public interface ISatellitePingEndpoint
{
    /// <summary>
    /// Processes the ping of the satellite with the given name and returns the answer: <c>204</c> if the ping is
    /// accepted, <c>400</c> if the body is invalid, <c>401</c> if the token is missing or rejected and <c>404</c> if no
    /// satellite has the name.
    /// </summary>
    Task<IResult> HandleAsync(string satelliteName, HttpRequest request, CancellationToken cancellationToken);
}
