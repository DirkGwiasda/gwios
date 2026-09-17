namespace GwiOS.Core.Tests.Satellites.Infrastructure.SatelliteMeasurementHttpClient;

/// <summary>
/// Stands in for <see cref="IHttpClientFactory"/>: hands out clients that send every request to <see cref="Handler"/>
/// and records the names the clients were requested with.
/// </summary>
public sealed class HttpClientFactoryFake : IHttpClientFactory
{
    private readonly List<string> _clientNames = [];

    /// <summary>
    /// The handler that answers the requests.
    /// </summary>
    public HttpMessageHandlerFake Handler { get; } = new();

    /// <summary>
    /// The names of all requested clients, in order.
    /// </summary>
    public IReadOnlyList<string> ClientNames
        => _clientNames;

    public HttpClient CreateClient(string name)
    {
        _clientNames.Add(name);
        return new HttpClient(Handler, disposeHandler: false);
    }
}
