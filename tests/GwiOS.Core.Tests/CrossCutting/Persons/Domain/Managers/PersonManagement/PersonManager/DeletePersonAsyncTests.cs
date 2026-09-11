using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.CrossCutting.Persons.Domain.Managers.PersonManagement.Contracts.Models;

namespace GwiOS.Core.Tests.CrossCutting.Persons.Domain.Managers.PersonManagement.PersonManager;

/// <summary>
/// Covers <c>PersonManager.DeletePersonAsync</c>.
/// </summary>
public sealed class DeletePersonAsyncTests
{
    private readonly PersonManagerProbe _probe = new();
    private readonly Person _anna = new() { Name = "Anna", ShortName = "A" };
    private readonly Person _bernd = new() { Name = "Bernd", ShortName = "B" };

    [Fact]
    public async Task DeletesOnlyThePersonWithTheGivenId()
    {
        await _probe.PersonRepository.InsertAsync(_anna, TestContext.Current.CancellationToken);
        await _probe.PersonRepository.InsertAsync(_bernd, TestContext.Current.CancellationToken);

        await _probe.PersonManager.DeletePersonAsync(_anna.Id, TestContext.Current.CancellationToken);

        List<Person> storedPersons = await _probe.PersonRepository.GetAllAsync(TestContext.Current.CancellationToken);
        Assert.Same(_bernd, Assert.Single(storedPersons));
    }

    [Fact]
    public async Task Succeeds_WhenThePersonIsNotStored()
    {
        Exception? exception = await Record.ExceptionAsync(
            () => _probe.PersonManager.DeletePersonAsync(_anna.Id, TestContext.Current.CancellationToken));

        Assert.Null(exception);
    }

    [Fact]
    public async Task LogsTheDeletionWithThePersonId()
    {
        await _probe.PersonRepository.InsertAsync(_anna, TestContext.Current.CancellationToken);

        await _probe.PersonManager.DeletePersonAsync(_anna.Id, TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_probe.Logger.Entries);
        Assert.Equal(LogLevel.Information, logEntry.LogLevel);
        Assert.Equal("Person deleted.", logEntry.Message);
        Assert.Equal(_anna.Id.ToString(), logEntry.ContextData["PersonId"]);
    }
}
