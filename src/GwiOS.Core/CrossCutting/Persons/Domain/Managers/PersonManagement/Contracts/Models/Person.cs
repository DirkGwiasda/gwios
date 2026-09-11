namespace GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;

/// <summary>
/// A person known to GwiOS, optionally linked to the identity user account the person signs in with.
/// </summary>
public class Person
{
    /// <summary>
    /// The unique identifier of the person. Defaults to a new time-ordered UUID (version 7).
    /// </summary>
    public Guid Id { get; init; } = Guid.CreateVersion7();

    /// <summary>
    /// The name of the person. Must not be empty and has to be unique across all persons in the system.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The short name of the person. Must not be empty and has to be unique across all persons in the system.
    /// </summary>
    public required string ShortName { get; set; }

    /// <summary>
    /// The ID of the identity user account the person signs in with, or <c>null</c> if the person has no user
    /// account. If set, it must not be empty and has to be unique across all persons in the system.
    /// </summary>
    public string? IdentityUserId { get; set; }
}
