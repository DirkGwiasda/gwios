using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;

namespace GwiOS.Core.Satellites.Infrastructure;

/// <summary>
/// Fetches measurements from the HTTP server of a satellite over plain HTTP in the LAN, using the named
/// <see cref="HttpClient"/> <see cref="HttpClientName"/>.
/// </summary>
internal sealed class SatelliteMeasurementHttpClient(
    IHttpClientFactory httpClientFactory,
    ILogger<SatelliteMeasurementHttpClient> logger) : ISatelliteMeasurementClient
{
    /// <summary>
    /// The name of the <see cref="HttpClient"/> configuration used for satellites.
    /// </summary>
    public const string HttpClientName = "GwiOS.Satellites";

    private const string MeasurementsPath = "/measurements";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<SatelliteMeasurementHttpClient> _logger = logger;

    public async Task<MeasurementReading> GetReadingAsync(
        string satelliteName,
        string endpoint,
        string token,
        CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = CreateRequest(endpoint, token);
        HttpClient httpClient = _httpClientFactory.CreateClient(HttpClientName);
        try
        {
            using HttpResponseMessage response = await httpClient.SendAsync(request, cancellationToken);
            return await ReadReadingAsync(satelliteName, response, cancellationToken);
        }
        catch (Exception exception) when (IsFetchFailure(exception, cancellationToken))
        {
            throw new MeasurementFetchException(satelliteName, DescribeFailure(exception), exception);
        }
    }

    private static HttpRequestMessage CreateRequest(string endpoint, string token)
    {
        HttpRequestMessage request = new(HttpMethod.Get, new Uri(new Uri(endpoint), MeasurementsPath));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private async Task<MeasurementReading> ReadReadingAsync(
        string satelliteName,
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.StatusCode != HttpStatusCode.OK)
        {
            LogUnexpectedStatus(satelliteName, response.StatusCode);
            throw new MeasurementFetchException(
                satelliteName,
                $"The satellite answered with status {(int)response.StatusCode}.");
        }

        ReadingResponse readingResponse =
            await response.Content.ReadFromJsonAsync<ReadingResponse>(SerializerOptions, cancellationToken)
            ?? throw new MeasurementFetchException(satelliteName, "The satellite answered with an empty response.");
        return ToReading(readingResponse);
    }

    // Successful fetches are not logged, because they happen every 30 seconds per satellite.
    private void LogUnexpectedStatus(string satelliteName, HttpStatusCode statusCode)
        => _logger.LogDebug(
            "Satellite answered the measurement request with an unexpected status.",
            new Dictionary<string, string> { ["SatelliteName"] = satelliteName, ["StatusCode"] = ((int)statusCode).ToString() });

    // A cancellation requested by the caller is no failure of the satellite and passes through unchanged.
    private static bool IsFetchFailure(Exception exception, CancellationToken cancellationToken)
        => exception switch
        {
            HttpRequestException => true,
            JsonException => true,
            NotSupportedException => true,
            TaskCanceledException => !cancellationToken.IsCancellationRequested,
            _ => false
        };

    private static string DescribeFailure(Exception exception)
        => exception switch
        {
            TaskCanceledException => "The satellite did not answer in time.",
            JsonException or NotSupportedException => "The response of the satellite cannot be read.",
            _ => "The satellite is not reachable."
        };

    private static MeasurementReading ToReading(ReadingResponse readingResponse)
        => new()
        {
            Measurements = [.. (readingResponse.Measurements ?? []).Select(ToMeasurement)],
            AgeSeconds = readingResponse.AgeSeconds
        };

    private static Measurement ToMeasurement(MeasurementResponse measurementResponse)
        => new()
        {
            Sensor = measurementResponse.Sensor ?? string.Empty,
            Quantity = measurementResponse.Quantity ?? string.Empty,
            Value = measurementResponse.Value,
            Unit = measurementResponse.Unit ?? string.Empty
        };

    private sealed record ReadingResponse(List<MeasurementResponse>? Measurements, int AgeSeconds);

    private sealed record MeasurementResponse(string? Sensor, string? Quantity, double Value, string? Unit);
}
