using GwiOS.Core.CrossCutting.Logging;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.Core.Tests.CrossCutting.Logging.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace GwiOS.Core.Tests.CrossCutting.Logging.DefaultLogger;

/// <summary>
/// Runs a <see cref="DefaultLogger{T}"/> against an in-memory repository and hands out the log entry it writes.
/// </summary>
public sealed class DefaultLoggerProbe : IDisposable
{
    private readonly LogEntryRepositoryFake _logEntryRepository = new();
    private readonly ServiceProvider _serviceProvider;

    public DefaultLoggerProbe()
    {
        ServiceCollection services = new();
        services.AddSingleton<ILogEntryRepository>(_logEntryRepository);
        _serviceProvider = services.BuildServiceProvider();
        Logger = new DefaultLogger<DefaultLoggerProbe>(_serviceProvider.GetRequiredService<IServiceScopeFactory>());
    }

    /// <summary>
    /// The data source every entry of <see cref="Logger"/> is expected to carry.
    /// </summary>
    public static string ExpectedDataSource { get; } = typeof(DefaultLoggerProbe).FullName!;

    /// <summary>
    /// The logger under test.
    /// </summary>
    public ILogger<DefaultLoggerProbe> Logger { get; }

    /// <summary>
    /// Waits until the logger has written its first entry and returns it. Fails after five seconds.
    /// </summary>
    public Task<LogEntry> WaitForWrittenEntryAsync()
        => _logEntryRepository.WaitForFirstInsertAsync();

    public void Dispose()
        => _serviceProvider.Dispose();
}
