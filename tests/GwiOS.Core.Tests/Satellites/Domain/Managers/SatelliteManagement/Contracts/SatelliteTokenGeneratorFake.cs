using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.Contracts;

/// <summary>
/// Stands in for <see cref="ISatelliteTokenGenerator"/> with predictable tokens: <c>generated-token-1</c>,
/// <c>generated-token-2</c> and so on.
/// </summary>
public sealed class SatelliteTokenGeneratorFake : ISatelliteTokenGenerator
{
    private int _generatedTokenCount;

    public string GenerateToken()
        => $"generated-token-{++_generatedTokenCount}";
}
