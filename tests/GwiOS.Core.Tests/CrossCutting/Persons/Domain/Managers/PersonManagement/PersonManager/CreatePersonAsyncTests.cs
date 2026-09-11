using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;

namespace GwiOS.Core.Tests.CrossCutting.Persons.Domain.Managers.PersonManagement.PersonManager;

/// <summary>
/// Covers <c>PersonManager.CreatePersonAsync</c>.
/// </summary>
public sealed class CreatePersonAsyncTests
{
    private readonly PersonManagerProbe _probe = new();
    private readonly Person _person = new() { Name = "Anna Beispiel", ShortName = "Anna", IdentityUserId = "user-1" };

    [Fact]
    public async Task StoresThePerson_WhenItIsValid()
    {
        await _probe.PersonManager.CreatePersonAsync(_person, TestContext.Current.CancellationToken);

        List<Person> storedPersons = await _probe.PersonRepository.GetAllAsync(TestContext.Current.CancellationToken);
        Assert.Same(_person, Assert.Single(storedPersons));
    }

    [Fact]
    public async Task ReturnsTheStoredPerson()
    {
        Person createdPerson =
            await _probe.PersonManager.CreatePersonAsync(_person, TestContext.Current.CancellationToken);

        Assert.Same(_person, createdPerson);
    }

    [Fact]
    public async Task ValidatesThePerson()
    {
        await _probe.PersonManager.CreatePersonAsync(_person, TestContext.Current.CancellationToken);

        Assert.Same(_person, Assert.Single(_probe.PersonValidator.ValidatedPersons));
    }

    [Fact]
    public async Task ThrowsPersonValidationExceptionWithTheValidationErrors_WhenThePersonIsInvalid()
    {
        _probe.PersonValidator.ValidationErrors = ["First error.", "Second error."];

        PersonValidationException exception = await Assert.ThrowsAsync<PersonValidationException>(
            () => _probe.PersonManager.CreatePersonAsync(_person, TestContext.Current.CancellationToken));

        Assert.Equal(_person.Id, exception.PersonId);
        Assert.Equal(["First error.", "Second error."], exception.ValidationErrors);
    }

    [Fact]
    public async Task DoesNotStoreThePerson_WhenItIsInvalid()
    {
        _probe.PersonValidator.ValidationErrors = ["Error."];

        await Assert.ThrowsAsync<PersonValidationException>(
            () => _probe.PersonManager.CreatePersonAsync(_person, TestContext.Current.CancellationToken));

        Assert.Empty(await _probe.PersonRepository.GetAllAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LogsTheCreationWithThePersonId()
    {
        await _probe.PersonManager.CreatePersonAsync(_person, TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Person created.", logEntry.Message);
        Assert.Equal(_person.Id.ToString(), logEntry.ContextData["PersonId"]);
    }
}
