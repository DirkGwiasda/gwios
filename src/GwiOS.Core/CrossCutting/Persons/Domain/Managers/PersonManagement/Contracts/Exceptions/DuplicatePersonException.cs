namespace GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions;

/// <summary>
/// Thrown when a person cannot be stored because one of its unique values is already used by another person.
/// </summary>
/// <param name="personId">The ID of the person that could not be stored.</param>
/// <param name="propertyName">
/// The name of the <c>Person</c> property whose value is already used: <c>Id</c>, <c>Name</c>, <c>ShortName</c> or
/// <c>IdentityUserId</c>.
/// </param>
/// <param name="innerException">The storage error that revealed the conflict.</param>
public sealed class DuplicatePersonException(Guid personId, string propertyName, Exception innerException)
    : Exception(
        $"Person '{personId}' cannot be stored because its {propertyName} is already used by another person.",
        innerException)
{
    /// <summary>
    /// The ID of the person that could not be stored.
    /// </summary>
    public Guid PersonId { get; } = personId;

    /// <summary>
    /// The name of the <c>Person</c> property whose value is already used: <c>Id</c>, <c>Name</c>, <c>ShortName</c>
    /// or <c>IdentityUserId</c>.
    /// </summary>
    public string PropertyName { get; } = propertyName;
}
