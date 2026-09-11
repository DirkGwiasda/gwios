namespace GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when a person is rejected because it violates at least one validation rule.
/// </summary>
/// <param name="personId">The ID of the rejected person.</param>
/// <param name="validationErrors">One message per violated rule; never empty.</param>
public sealed class PersonValidationException(Guid personId, IReadOnlyList<string> validationErrors)
    : Exception($"Person '{personId}' is invalid: {string.Join(" ", validationErrors)}")
{
    /// <summary>
    /// The ID of the rejected person.
    /// </summary>
    public Guid PersonId { get; } = personId;

    /// <summary>
    /// One message per violated rule; never empty.
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; } = validationErrors;
}
