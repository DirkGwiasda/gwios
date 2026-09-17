using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.SatelliteManager;

/// <summary>
/// Covers <c>SatelliteManager.ChangeDisplayNameAsync</c>.
/// </summary>
public sealed class ChangeDisplayNameAsyncTests
{
    private readonly SatelliteManagerProbe _probe = new();

    [Fact]
    public async Task StoresTheTrimmedDisplayName()
    {
        _probe.AddSatellite("keller");

        await _probe.SatelliteManager.ChangeDisplayNameAsync("keller", " Keller ", TestContext.Current.CancellationToken);

        Assert.Equal("Keller", _probe.GetStoredSatellite("keller").DisplayName);
    }

    [Fact]
    public async Task RemovesTheDisplayName_WhenItIsEmpty()
    {
        _probe.AddSatellite("keller").DisplayName = "Keller";

        await _probe.SatelliteManager.ChangeDisplayNameAsync("keller", "", TestContext.Current.CancellationToken);

        Assert.Null(_probe.GetStoredSatellite("keller").DisplayName);
    }

    [Fact]
    public async Task ThrowsSatelliteNotFoundException_WhenNoSuchSatelliteIsStored()
    {
        SatelliteNotFoundException exception = await Assert.ThrowsAsync<SatelliteNotFoundException>(
            () => _probe.SatelliteManager.ChangeDisplayNameAsync("keller", "Keller", TestContext.Current.CancellationToken));

        Assert.Equal("keller", exception.SatelliteName);
    }

    [Fact]
    public async Task ThrowsSatelliteValidationException_WhenTheDisplayNameIsInvalid()
    {
        _probe.AddSatellite("keller");
        _probe.SatelliteValidator.ValidationErrors = ["Invalid display name."];

        await Assert.ThrowsAsync<SatelliteValidationException>(
            () => _probe.SatelliteManager.ChangeDisplayNameAsync("keller", "x", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LogsTheChange()
    {
        _probe.AddSatellite("keller");

        await _probe.SatelliteManager.ChangeDisplayNameAsync("keller", "Keller", TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal("Satellite display name changed.", logEntry.Message);
        Assert.Equal("keller", logEntry.ContextData["SatelliteName"]);
    }
}
