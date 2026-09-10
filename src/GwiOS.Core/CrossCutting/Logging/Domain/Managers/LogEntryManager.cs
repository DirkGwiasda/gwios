using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

namespace GwiOS.Core.CrossCutting.Logging.Domain.Managers;

internal sealed class LogEntryManager(ILogEntryRepository repository) : ILogEntryManager
{
    public async Task<List<string>> GetAllAppNamesAsync()
        => await repository.GetAllAppNamesAsync();

    public async Task<List<LogEntry>> GetAllLogEntriesByAppAsync(string appName)
        => await repository.GetAllLogEntriesByAppAsync(appName);

    public async Task DeleteByAppNameAsync(string appName)
        => await repository.DeleteByAppNameAsync(appName);
}
