using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

namespace GwiOS.Core.CrossCutting.Logging.Domain.Managers;

internal sealed class LogEntryManager(ILogEntryRepository logEntryRepository) : ILogEntryManager
{
    private readonly ILogEntryRepository _logEntryRepository = logEntryRepository;

    public async Task<List<string>> GetAllAppNamesAsync()
        => await _logEntryRepository.GetAllAppNamesAsync();

    public async Task<List<LogEntry>> GetAllLogEntriesByAppAsync(string appName)
        => await _logEntryRepository.GetAllLogEntriesByAppAsync(appName);

    public async Task DeleteByAppNameAsync(string appName)
        => await _logEntryRepository.DeleteByAppNameAsync(appName);
}
