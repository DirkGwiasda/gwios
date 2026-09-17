using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts;
using GwiOS.Core.Satellites.Domain.Managers.SatelliteManagement.Contracts.Models;

namespace GwiOS.Core.Tests.Satellites.Domain.Managers.SatelliteManagement.Contracts;

/// <summary>
/// Stands in for <see cref="ISatelliteValidator"/>. It considers every satellite and token valid until a test sets
/// <see cref="ValidationErrors"/> or <see cref="IsTokenValid"/>, and records what it validates.
/// </summary>
public sealed class SatelliteValidatorFake : ISatelliteValidator
{
    private readonly List<Satellite> _validatedSatellites = [];
    private readonly List<string> _validatedTokens = [];

    /// <summary>
    /// The validation errors reported for every satellite. Empty by default, which makes every satellite valid.
    /// </summary>
    public List<string> ValidationErrors { get; set; } = [];

    /// <summary>
    /// Whether every token is valid. <c>true</c> by default.
    /// </summary>
    public bool IsTokenValid { get; set; } = true;

    /// <summary>
    /// All satellites validated so far, in the order of validation.
    /// </summary>
    public IReadOnlyList<Satellite> ValidatedSatellites
        => _validatedSatellites;

    /// <summary>
    /// All tokens validated so far, in the order of validation.
    /// </summary>
    public IReadOnlyList<string> ValidatedTokens
        => _validatedTokens;

    public bool IsValid(Satellite satellite, out List<string> validationErrors)
    {
        _validatedSatellites.Add(satellite);
        validationErrors = [.. ValidationErrors];
        return validationErrors.Count == 0;
    }

    public bool IsValidToken(string token)
    {
        _validatedTokens.Add(token);
        return IsTokenValid;
    }
}
