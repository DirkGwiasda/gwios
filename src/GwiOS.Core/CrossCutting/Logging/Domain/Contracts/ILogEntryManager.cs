using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

namespace GwiOS.Core.CrossCutting.Logging.Domain.Contracts;

public interface ILogEntryManager
{
    Task<List<string>> GetAllAppNamesAsync();
    Task<List<LogEntry>> GetAllLogEntriesByAppAsync(string appName);
    Task DeleteByAppNameAsync(string appName);
}
