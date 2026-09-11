using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;

namespace GwiOS.Core.Tests.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;

/// <summary>
/// Stands in for <see cref="IPersonValidator"/>. It considers every person valid until a test sets
/// <see cref="ValidationErrors"/>, and records every person it validates.
/// </summary>
public sealed class PersonValidatorFake : IPersonValidator
{
    private readonly List<Person> _validatedPersons = [];

    /// <summary>
    /// The validation errors reported for every person. Empty by default, which makes every person valid.
    /// </summary>
    public List<string> ValidationErrors { get; set; } = [];

    /// <summary>
    /// All persons validated so far, in the order of validation.
    /// </summary>
    public IReadOnlyList<Person> ValidatedPersons
        => _validatedPersons;

    public bool IsValid(Person person, out List<string> validationErrors)
    {
        _validatedPersons.Add(person);
        validationErrors = [.. ValidationErrors];
        return validationErrors.Count == 0;
    }
}
