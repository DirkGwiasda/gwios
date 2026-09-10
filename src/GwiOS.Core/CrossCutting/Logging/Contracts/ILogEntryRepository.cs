using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

namespace GwiOS.Core.CrossCutting.Logging.Contracts;

public interface ILogEntryRepository
{
    Task InsertAsync(LogEntry logEntry);
    Task<List<string>> GetAllAppNamesAsync();
    Task<List<LogEntry>> GetAllLogEntriesByAppAsync(string appName);
    Task DeleteByAppNameAsync(string appName);
}