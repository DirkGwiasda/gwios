using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;

namespace GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement;

internal sealed class PersonValidator(ILogger<PersonValidator> logger) : IPersonValidator
{
    private readonly ILogger<PersonValidator> _logger = logger;

    public bool IsValid(Person person, out List<string> validationErrors)
    {
        validationErrors = CollectValidationErrors(person);
        if (validationErrors.Count == 0)
        {
            return true;
        }

        LogValidationErrors(person, validationErrors);
        return false;
    }

    private static List<string> CollectValidationErrors(Person person)
    {
        List<string> validationErrors = [];
        if (string.IsNullOrWhiteSpace(person.Name))
        {
            validationErrors.Add("The name must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(person.ShortName))
        {
            validationErrors.Add("The short name must not be empty.");
        }

        if ((person.IdentityUserId is not null) && string.IsNullOrWhiteSpace(person.IdentityUserId))
        {
            validationErrors.Add(
                "The identity user ID must not be empty; it is null if the person has no user account.");
        }

        return validationErrors;
    }

    private void LogValidationErrors(Person person, List<string> validationErrors)
        => _logger.LogWarning(
            "Person is invalid.",
            new Dictionary<string, string>
            {
                ["PersonId"] = person.Id.ToString(),
                ["ValidationErrors"] = string.Join(" ", validationErrors)
            });
}
