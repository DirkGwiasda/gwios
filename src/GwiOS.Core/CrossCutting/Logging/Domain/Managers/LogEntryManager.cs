using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

namespace GwiOS.Core.CrossCutting.Logging.Domain.Managers;

internal sealed class LogEntryManager(ILogEntryRepository logEntryRepository) : ILogEntryManager
{
    private readonly ILogEntryRepository _logEntryRepository = logEntryRepository;

    public async Task<List<string>> GetAllAppNamesAsync()
        => await _logEntryRepository.GetAllAppNamesAsync();

    public async Task<LogEntryPage> GetLogEntryPageAsync(int pageNumber, int pageSize)
    {
        ThrowIfPageIsInvalid(pageNumber, pageSize);
        return await _logEntryRepository.GetLogEntryPageAsync(pageNumber, pageSize);
    }

    public async Task<LogEntryPage> GetLogEntryPageByAppAsync(string appName, int pageNumber, int pageSize)
    {
        ThrowIfPageIsInvalid(pageNumber, pageSize);
        return await _logEntryRepository.GetLogEntryPageByAppAsync(appName, pageNumber, pageSize);
    }

    public async Task DeleteByAppNameAsync(string appName)
        => await _logEntryRepository.DeleteByAppNameAsync(appName);

    private static void ThrowIfPageIsInvalid(int pageNumber, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
    }
}
