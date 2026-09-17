using System.Net.Http.Headers;
using System.Text.Json;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Workflows.SatellitePing.Contracts;
using GwiOS.Core.Satellites.Domain.Workflows.SatellitePing.Contracts.Models;
using GwiOS.WebUI.Api.Satellites.Contracts;
using GwiOS.WebUI.Api.Satellites.Contracts.Models;
using Microsoft.Net.Http.Headers;

namespace GwiOS.WebUI.Api.Satellites;

internal sealed class SatellitePingEndpoint(
    ISatellitePingWorkflow satellitePingWorkflow,
    ILogger<SatellitePingEndpoint> logger) : ISatellitePingEndpoint
{
    private const string BearerScheme = "Bearer";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly ISatellitePingWorkflow _satellitePingWorkflow = satellitePingWorkflow;
    private readonly ILogger<SatellitePingEndpoint> _logger = logger;

    public async Task<IResult> HandleAsync(
        string satelliteName,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (ReadBearerToken(request) is not { } token)
        {
            return CreateUnauthorizedResult(request);
        }

        if (await ReadBodyAsync(request, cancellationToken) is not { } body)
        {
            return Results.BadRequest();
        }

        return await ProcessPingAsync(CreatePing(satelliteName, token, body), request, cancellationToken);
    }

    private async Task<IResult> ProcessPingAsync(
        SatellitePingData ping,
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _satellitePingWorkflow.ProcessPingAsync(ping, cancellationToken);
            return Results.NoContent();
        }
        catch (SatelliteNotFoundException)
        {
            return Results.NotFound();
        }
        catch (SatelliteTokenRejectedException)
        {
            return CreateUnauthorizedResult(request);
        }
        catch (SatellitePresenceValidationException)
        {
            return Results.BadRequest();
        }
    }

    // The scheme is case-insensitive by the HTTP rules; the token itself is checked by the workflow.
    private static string? ReadBearerToken(HttpRequest request)
        => AuthenticationHeaderValue.TryParse(request.Headers.Authorization.ToString(), out AuthenticationHeaderValue? header)
            && string.Equals(header.Scheme, BearerScheme, StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrEmpty(header.Parameter)
                ? header.Parameter
                : null;

    private async Task<SatellitePingRequest?> ReadBodyAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        try
        {
            SatellitePingRequest? body = await JsonSerializer.DeserializeAsync<SatellitePingRequest>(
                request.Body,
                SerializerOptions,
                cancellationToken);
            return IsComplete(body) ? body : null;
        }
        catch (JsonException)
        {
            _logger.LogWarning("Satellite ping has a body that is no valid JSON.");
            return null;
        }
    }

    private static bool IsComplete(SatellitePingRequest? body)
        => (body?.Endpoint is not null)
            && (body.Board is not null)
            && (body.Firmware is not null)
            && (body.WifiRssiDbm is not null)
            && (body.Sensors is not null)
            && body.Sensors.All(sensor => (sensor.Type is not null) && (sensor.Quantities is not null));

    private static SatellitePingData CreatePing(string satelliteName, string token, SatellitePingRequest body)
        => new()
        {
            SatelliteName = satelliteName,
            Token = token,
            Endpoint = body.Endpoint!,
            Board = body.Board!,
            Firmware = body.Firmware!,
            WifiRssiDbm = body.WifiRssiDbm!.Value,
            Sensors = [.. body.Sensors!.Select(ToSensor)]
        };

    private static SatellitePingSensor ToSensor(SatellitePingSensorRequest sensor)
        => new() { Type = sensor.Type!, Quantities = [.. sensor.Quantities!] };

    private static IResult CreateUnauthorizedResult(HttpRequest request)
    {
        request.HttpContext.Response.Headers[HeaderNames.WWWAuthenticate] = BearerScheme;
        return Results.Unauthorized();
    }
}
