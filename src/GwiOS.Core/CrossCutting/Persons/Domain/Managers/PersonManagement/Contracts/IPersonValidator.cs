using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;

namespace GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;

/// <summary>
/// Checks the values of a single person. Uniqueness across persons is not part of the check; the repository
/// enforces it when the person is stored.
/// </summary>
public interface IPersonValidator
{
    /// <summary>
    /// Returns whether the given person is valid. <paramref name="validationErrors"/> receives one message per
    /// violated rule; it is empty if the person is valid.
    /// </summary>
    bool IsValid(Person person, out List<string> validationErrors);
}
