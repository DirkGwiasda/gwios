using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Exceptions;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.SatelliteManager;

/// <summary>
/// Covers <c>SatelliteManager.CreateLockedSatelliteAsync</c>.
/// </summary>
public sealed class CreateLockedSatelliteAsyncTests
{
    private readonly SatelliteManagerProbe _probe = new();

    [Fact]
    public async Task StoresALockedSatelliteWithoutToken()
    {
        await _probe.SatelliteManager.CreateLockedSatelliteAsync("fremd", TestContext.Current.CancellationToken);

        Satellite satellite = Assert.Single(_probe.SatelliteRepository.Satellites);
        Assert.Equal("fremd", satellite.Name);
        Assert.True(satellite.IsLocked);
        Assert.Null(satellite.ProtectedToken);
        Assert.Null(satellite.DisplayName);
    }

    [Fact]
    public async Task ThrowsSatelliteValidationException_WhenTheNameIsInvalid()
    {
        _probe.SatelliteValidator.ValidationErrors = ["Invalid name."];

        await Assert.ThrowsAsync<SatelliteValidationException>(
            () => _probe.SatelliteManager.CreateLockedSatelliteAsync("Fremd!", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ThrowsDuplicateSatelliteException_WhenTheNameIsAlreadyUsed()
    {
        _probe.AddSatellite("fremd");

        await Assert.ThrowsAsync<DuplicateSatelliteException>(
            () => _probe.SatelliteManager.CreateLockedSatelliteAsync("fremd", TestContext.Current.CancellationToken));
    }
}
