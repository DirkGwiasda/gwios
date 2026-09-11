namespace GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when an operation requires a stored person, but no person with the given ID is stored.
/// </summary>
/// <param name="personId">The ID that no stored person has.</param>
public sealed class PersonNotFoundException(Guid personId)
    : Exception($"No person with ID '{personId}' is stored.")
{
    /// <summary>
    /// The ID that no stored person has.
    /// </summary>
    public Guid PersonId { get; } = personId;
}
