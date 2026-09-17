namespace GwiOS.Core.Tests.Satellites.Infrastructure.SatelliteMeasurementHttpClient;

/// <summary>
/// Answers HTTP requests with <see cref="Respond"/> instead of sending them, and records the requests.
/// </summary>
public sealed class HttpMessageHandlerFake : HttpMessageHandler
{
    private readonly List<HttpRequestMessage> _requests = [];

    /// <summary>
    /// Creates the response to a request; may throw to simulate a network failure. Answers with <c>200</c> and an empty
    /// reading by default.
    /// </summary>
    public Func<HttpRequestMessage, HttpResponseMessage> Respond { get; set; } = _ => new HttpResponseMessage
    {
        Content = new StringContent("""{"measurements":[],"ageSeconds":0}""")
    };

    /// <summary>
    /// All received requests, oldest first.
    /// </summary>
    public IReadOnlyList<HttpRequestMessage> Requests
        => _requests;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _requests.Add(request);
        return Task.FromResult(Respond(request));
    }
}
