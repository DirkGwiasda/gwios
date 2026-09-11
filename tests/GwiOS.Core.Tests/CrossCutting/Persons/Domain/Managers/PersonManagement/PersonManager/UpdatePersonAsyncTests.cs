using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Exceptions;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;

namespace GwiOS.Core.Tests.CrossCutting.Persons.Domain.Managers.PersonManagement.PersonManager;

/// <summary>
/// Covers <c>PersonManager.UpdatePersonAsync</c>.
/// </summary>
public sealed class UpdatePersonAsyncTests
{
    private readonly PersonManagerProbe _probe = new();
    private readonly Person _storedPerson = new() { Name = "Anna Beispiel", ShortName = "Anna" };
    private readonly Person _changedPerson;

    public UpdatePersonAsyncTests()
    {
        _changedPerson = new Person { Id = _storedPerson.Id, Name = "Anna Muster", ShortName = "Anni" };
    }

    [Fact]
    public async Task OverwritesTheStoredPerson_WhenTheChangedPersonIsValid()
    {
        await _probe.PersonRepository.InsertAsync(_storedPerson, TestContext.Current.CancellationToken);

        await _probe.PersonManager.UpdatePersonAsync(_changedPerson, TestContext.Current.CancellationToken);

        List<Person> storedPersons = await _probe.PersonRepository.GetAllAsync(TestContext.Current.CancellationToken);
        Assert.Same(_changedPerson, Assert.Single(storedPersons));
    }

    [Fact]
    public async Task ValidatesTheChangedPerson()
    {
        await _probe.PersonRepository.InsertAsync(_storedPerson, TestContext.Current.CancellationToken);

        await _probe.PersonManager.UpdatePersonAsync(_changedPerson, TestContext.Current.CancellationToken);

        Assert.Same(_changedPerson, Assert.Single(_probe.PersonValidator.ValidatedPersons));
    }

    [Fact]
    public async Task ThrowsPersonValidationExceptionWithTheValidationErrors_WhenTheChangedPersonIsInvalid()
    {
        await _probe.PersonRepository.InsertAsync(_storedPerson, TestContext.Current.CancellationToken);
        _probe.PersonValidator.ValidationErrors = ["Error."];

        PersonValidationException exception = await Assert.ThrowsAsync<PersonValidationException>(
            () => _probe.PersonManager.UpdatePersonAsync(_changedPerson, TestContext.Current.CancellationToken));

        Assert.Equal(_changedPerson.Id, exception.PersonId);
        Assert.Equal(["Error."], exception.ValidationErrors);
    }

    [Fact]
    public async Task KeepsTheStoredPerson_WhenTheChangedPersonIsInvalid()
    {
        await _probe.PersonRepository.InsertAsync(_storedPerson, TestContext.Current.CancellationToken);
        _probe.PersonValidator.ValidationErrors = ["Error."];

        await Assert.ThrowsAsync<PersonValidationException>(
            () => _probe.PersonManager.UpdatePersonAsync(_changedPerson, TestContext.Current.CancellationToken));

        List<Person> storedPersons = await _probe.PersonRepository.GetAllAsync(TestContext.Current.CancellationToken);
        Assert.Same(_storedPerson, Assert.Single(storedPersons));
    }

    [Fact]
    public async Task ThrowsPersonNotFoundException_WhenThePersonIsNotStored()
    {
        PersonNotFoundException exception = await Assert.ThrowsAsync<PersonNotFoundException>(
            () => _probe.PersonManager.UpdatePersonAsync(_changedPerson, TestContext.Current.CancellationToken));

        Assert.Equal(_changedPerson.Id, exception.PersonId);
    }

    [Fact]
    public async Task LogsTheUpdateWithThePersonId()
    {
        await _probe.PersonRepository.InsertAsync(_storedPerson, TestContext.Current.CancellationToken);

        await _probe.PersonManager.UpdatePersonAsync(_changedPerson, TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Person updated.", logEntry.Message);
        Assert.Equal(_changedPerson.Id.ToString(), logEntry.ContextData["PersonId"]);
    }
}
