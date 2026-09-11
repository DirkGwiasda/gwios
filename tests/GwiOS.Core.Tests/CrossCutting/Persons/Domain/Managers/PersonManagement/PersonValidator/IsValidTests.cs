using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using PersonValidatorUnderTest = GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.PersonValidator;

namespace GwiOS.Core.Tests.CrossCutting.Persons.Domain.Managers.PersonManagement.PersonValidator;

/// <summary>
/// Covers <c>PersonValidator.IsValid</c>.
/// </summary>
public sealed class IsValidTests
{
    private readonly LoggerFake<PersonValidatorUnderTest> _logger = new();
    private readonly PersonValidatorUnderTest _personValidator;

    public IsValidTests()
    {
        _personValidator = new PersonValidatorUnderTest(_logger);
    }

    [Fact]
    public void ReturnsTrueWithoutValidationErrors_WhenAllValuesAreSet()
    {
        Person person = new() { Name = "Anna Beispiel", ShortName = "Anna", IdentityUserId = "user-1" };

        bool isValid = _personValidator.IsValid(person, out List<string> validationErrors);

        Assert.True(isValid);
        Assert.Empty(validationErrors);
    }

    [Fact]
    public void ReturnsTrue_WhenThePersonHasNoIdentityUserId()
    {
        Person person = new() { Name = "Anna Beispiel", ShortName = "Anna" };

        bool isValid = _personValidator.IsValid(person, out _);

        Assert.True(isValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnsFalseWithAnError_WhenTheNameIsEmpty(string name)
    {
        Person person = new() { Name = name, ShortName = "Anna" };

        bool isValid = _personValidator.IsValid(person, out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Equal(["The name must not be empty."], validationErrors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnsFalseWithAnError_WhenTheShortNameIsEmpty(string shortName)
    {
        Person person = new() { Name = "Anna Beispiel", ShortName = shortName };

        bool isValid = _personValidator.IsValid(person, out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Equal(["The short name must not be empty."], validationErrors);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ReturnsFalseWithAnError_WhenTheIdentityUserIdIsEmpty(string identityUserId)
    {
        Person person = new() { Name = "Anna Beispiel", ShortName = "Anna", IdentityUserId = identityUserId };

        bool isValid = _personValidator.IsValid(person, out List<string> validationErrors);

        Assert.False(isValid);
        Assert.Equal(
            ["The identity user ID must not be empty; it is null if the person has no user account."],
            validationErrors);
    }

    [Fact]
    public void ReportsOneErrorPerViolatedRule()
    {
        Person person = new() { Name = "", ShortName = "", IdentityUserId = "" };

        _personValidator.IsValid(person, out List<string> validationErrors);

        Assert.Equal(3, validationErrors.Count);
    }

    [Fact]
    public void LogsAWarningWithThePersonIdAndTheErrors_WhenThePersonIsInvalid()
    {
        Person person = new() { Name = "", ShortName = "Anna" };

        _personValidator.IsValid(person, out _);

        LogEntry logEntry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Warning, logEntry.LogLevel);
        Assert.Equal(person.Id.ToString(), logEntry.ContextData["PersonId"]);
        Assert.Equal("The name must not be empty.", logEntry.ContextData["ValidationErrors"]);
    }

    [Fact]
    public void LogsNothing_WhenThePersonIsValid()
    {
        Person person = new() { Name = "Anna Beispiel", ShortName = "Anna" };

        _personValidator.IsValid(person, out _);

        Assert.Empty(_logger.Entries);
    }
}
