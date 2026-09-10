using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;

namespace GwiOS.Core.CrossCutting.Logging.Infrastructure;

internal sealed class LogEntryPostgresRepository : ILogEntryRepository
{
    public Task DeleteByAppNameAsync(string appName)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetAllAppNamesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<List<LogEntry>> GetAllLogEntriesByAppAsync(string appName)
    {
        throw new NotImplementedException();
    }

    public Task InsertAsync(LogEntry logEntry)
    {
        throw new NotImplementedException();
    }
}
