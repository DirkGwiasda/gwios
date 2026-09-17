using System.Net;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.MeasurementManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using SatelliteMeasurementHttpClientUnderTest = GwiOS.Core.Satellites.Infrastructure.SatelliteMeasurementHttpClient;

namespace GwiOS.Core.Tests.Satellites.Infrastructure.SatelliteMeasurementHttpClient;

/// <summary>
/// Covers <c>SatelliteMeasurementHttpClient.GetReadingAsync</c> with a handler that answers instead of the satellite.
/// </summary>
public sealed class GetReadingAsyncTests
{
    private readonly HttpClientFactoryFake _httpClientFactory = new();
    private readonly SatelliteMeasurementHttpClientUnderTest _client;

    public GetReadingAsyncTests()
    {
        _client = new SatelliteMeasurementHttpClientUnderTest(
            _httpClientFactory,
            new LoggerFake<SatelliteMeasurementHttpClientUnderTest>());
    }

    [Fact]
    public async Task RequestsTheMeasurementsOfTheEndpointWithTheTokenAsBearerToken()
    {
        await _client.GetReadingAsync("keller", "http://192.168.178.33", "my-token", TestContext.Current.CancellationToken);

        HttpRequestMessage request = Assert.Single(_httpClientFactory.Handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal(new Uri("http://192.168.178.33/measurements"), request.RequestUri);
        Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
        Assert.Equal("my-token", request.Headers.Authorization?.Parameter);
        Assert.Equal(["GwiOS.Satellites"], _httpClientFactory.ClientNames);
    }

    [Fact]
    public async Task ReturnsTheReadingOfTheSatellite()
    {
        _httpClientFactory.Handler.Respond = _ => new HttpResponseMessage
        {
            Content = new StringContent(
                """{"measurements":[{"sensor":"bme280","quantity":"temperature","value":24.20,"unit":"°C"},{"sensor":"bme280","quantity":"pressure","value":996.37,"unit":"hPa"}],"ageSeconds":3}""")
        };

        MeasurementReading reading = await _client.GetReadingAsync(
            "keller",
            "http://192.168.178.33",
            "my-token",
            TestContext.Current.CancellationToken);

        Assert.Equal(3, reading.AgeSeconds);
        Assert.Equal(
            [("bme280", "temperature", 24.20, "°C"), ("bme280", "pressure", 996.37, "hPa")],
            reading.Measurements.Select(measurement => (measurement.Sensor, measurement.Quantity, measurement.Value, measurement.Unit)));
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task ThrowsMeasurementFetchException_WhenTheSatelliteAnswersWithAnotherStatus(HttpStatusCode statusCode)
    {
        _httpClientFactory.Handler.Respond = _ => new HttpResponseMessage(statusCode);

        MeasurementFetchException exception = await Assert.ThrowsAsync<MeasurementFetchException>(
            () => _client.GetReadingAsync("keller", "http://192.168.178.33", "my-token", TestContext.Current.CancellationToken));

        Assert.Equal("keller", exception.SatelliteName);
        Assert.Contains(((int)statusCode).ToString(), exception.Message);
        Assert.DoesNotContain("my-token", exception.Message);
    }

    [Theory]
    [InlineData("no json")]
    [InlineData("null")]
    [InlineData("""{"measurements":"wrong"}""")]
    public async Task ThrowsMeasurementFetchException_WhenTheResponseCannotBeRead(string content)
    {
        _httpClientFactory.Handler.Respond = _ => new HttpResponseMessage { Content = new StringContent(content) };

        await Assert.ThrowsAsync<MeasurementFetchException>(
            () => _client.GetReadingAsync("keller", "http://192.168.178.33", "my-token", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ThrowsMeasurementFetchException_WhenTheSatelliteIsNotReachable()
    {
        HttpRequestException networkError = new("Connection refused.");
        _httpClientFactory.Handler.Respond = _ => throw networkError;

        MeasurementFetchException exception = await Assert.ThrowsAsync<MeasurementFetchException>(
            () => _client.GetReadingAsync("keller", "http://192.168.178.33", "my-token", TestContext.Current.CancellationToken));

        Assert.Same(networkError, exception.InnerException);
    }

    [Fact]
    public async Task ThrowsMeasurementFetchException_WhenTheSatelliteDoesNotAnswerInTime()
    {
        _httpClientFactory.Handler.Respond = _ => throw new TaskCanceledException("Timeout.");

        MeasurementFetchException exception = await Assert.ThrowsAsync<MeasurementFetchException>(
            () => _client.GetReadingAsync("keller", "http://192.168.178.33", "my-token", TestContext.Current.CancellationToken));

        Assert.Contains("in time", exception.Message);
    }

    [Fact]
    public async Task LetsTheCancellationOfTheCallerPass()
    {
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        _httpClientFactory.Handler.Respond = _ => throw new TaskCanceledException();

        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _client.GetReadingAsync("keller", "http://192.168.178.33", "my-token", cancellation.Token));
    }
}
