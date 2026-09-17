using System.Text;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatellitePresenceManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Workflows.SatellitePing.Contracts.Models;
using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using SatellitePingEndpointUnderTest = GwiOS.WebUI.Api.Satellites.SatellitePingEndpoint;

namespace GwiOS.WebUI.Tests.Api.Satellites.SatellitePingEndpoint;

/// <summary>
/// Covers <c>SatellitePingEndpoint.HandleAsync</c>.
/// </summary>
public sealed class HandleAsyncTests
{
    private const string ValidBody = """
        {"endpoint":"http://192.168.178.33","board":"ESP32 DevKit C","firmware":"0.1.0","wifiRssiDbm":-70,"sensors":[{"type":"bme280","quantities":["temperature","humidity","pressure"]}]}
        """;

    private readonly SatellitePingWorkflowFake _workflow = new();
    private readonly LoggerFake<SatellitePingEndpointUnderTest> _logger = new();
    private readonly SatellitePingEndpointUnderTest _endpoint;

    public HandleAsyncTests()
    {
        _endpoint = new SatellitePingEndpointUnderTest(_workflow, _logger);
    }

    [Fact]
    public async Task AnswersNoContentAndPassesThePingOn_WhenThePingIsAccepted()
    {
        HttpRequest request = CreateRequest("Bearer my-token", ValidBody);

        IResult result = await _endpoint.HandleAsync("arbeitszimmer", request, Xunit.TestContext.Current.CancellationToken);

        Assert.IsType<NoContent>(result);
        SatellitePingData ping = Assert.Single(_workflow.Pings);
        Assert.Equal("arbeitszimmer", ping.SatelliteName);
        Assert.Equal("my-token", ping.Token);
        Assert.Equal("http://192.168.178.33", ping.Endpoint);
        Assert.Equal("ESP32 DevKit C", ping.Board);
        Assert.Equal("0.1.0", ping.Firmware);
        Assert.Equal(-70, ping.WifiRssiDbm);
        SatellitePingSensor sensor = Assert.Single(ping.Sensors);
        Assert.Equal("bme280", sensor.Type);
        Assert.Equal(["temperature", "humidity", "pressure"], sensor.Quantities);
    }

    [Theory]
    [InlineData("bearer my-token")]
    [InlineData("BEARER my-token")]
    public async Task AcceptsTheBearerSchemeInAnyCase(string authorization)
    {
        IResult result = await _endpoint.HandleAsync(
            "arbeitszimmer",
            CreateRequest(authorization, ValidBody),
            Xunit.TestContext.Current.CancellationToken);

        Assert.IsType<NoContent>(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Bearer")]
    [InlineData("Basic dXNlcjpwYXNz")]
    public async Task AnswersUnauthorizedWithoutAskingTheWorkflow_WhenNoBearerTokenIsSent(string? authorization)
    {
        HttpRequest request = CreateRequest(authorization, ValidBody);

        IResult result = await _endpoint.HandleAsync("arbeitszimmer", request, Xunit.TestContext.Current.CancellationToken);

        Assert.IsType<UnauthorizedHttpResult>(result);
        Assert.Equal("Bearer", request.HttpContext.Response.Headers.WWWAuthenticate.ToString());
        Assert.Empty(_workflow.Pings);
    }

    [Fact]
    public async Task AnswersUnauthorized_WhenTheTokenIsRejected()
    {
        _workflow.ExceptionToThrow = new SatelliteTokenRejectedException("arbeitszimmer");
        HttpRequest request = CreateRequest("Bearer wrong-token", ValidBody);

        IResult result = await _endpoint.HandleAsync("arbeitszimmer", request, Xunit.TestContext.Current.CancellationToken);

        Assert.IsType<UnauthorizedHttpResult>(result);
        Assert.Equal("Bearer", request.HttpContext.Response.Headers.WWWAuthenticate.ToString());
    }

    [Fact]
    public async Task AnswersNotFound_WhenTheSatelliteIsUnknown()
    {
        _workflow.ExceptionToThrow = new SatelliteNotFoundException("garage");

        IResult result = await _endpoint.HandleAsync(
            "garage",
            CreateRequest("Bearer my-token", ValidBody),
            Xunit.TestContext.Current.CancellationToken);

        Assert.IsType<NotFound>(result);
    }

    [Fact]
    public async Task AnswersBadRequest_WhenTheReportedValuesAreInvalid()
    {
        _workflow.ExceptionToThrow = new SatellitePresenceValidationException("arbeitszimmer", ["Invalid."]);

        IResult result = await _endpoint.HandleAsync(
            "arbeitszimmer",
            CreateRequest("Bearer my-token", ValidBody),
            Xunit.TestContext.Current.CancellationToken);

        Assert.IsType<BadRequest>(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("no json")]
    [InlineData("null")]
    [InlineData("""{"endpoint":"http://192.168.178.33","board":"ESP32","firmware":"0.1.0","sensors":[]}""")]
    [InlineData("""{"endpoint":"http://192.168.178.33","board":"ESP32","firmware":"0.1.0","wifiRssiDbm":-70}""")]
    [InlineData("""{"endpoint":"http://192.168.178.33","board":"ESP32","firmware":"0.1.0","wifiRssiDbm":-70,"sensors":[{"type":"bme280"}]}""")]
    [InlineData("""{"endpoint":"http://192.168.178.33","board":"ESP32","firmware":"0.1.0","wifiRssiDbm":"strong","sensors":[]}""")]
    public async Task AnswersBadRequestWithoutAskingTheWorkflow_WhenTheBodyIsNoCompletePing(string body)
    {
        IResult result = await _endpoint.HandleAsync(
            "arbeitszimmer",
            CreateRequest("Bearer my-token", body),
            Xunit.TestContext.Current.CancellationToken);

        Assert.IsType<BadRequest>(result);
        Assert.Empty(_workflow.Pings);
    }

    private static HttpRequest CreateRequest(string? authorization, string body)
    {
        DefaultHttpContext context = new();
        if (authorization is not null)
        {
            context.Request.Headers.Authorization = authorization;
        }

        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        return context.Request;
    }
}
