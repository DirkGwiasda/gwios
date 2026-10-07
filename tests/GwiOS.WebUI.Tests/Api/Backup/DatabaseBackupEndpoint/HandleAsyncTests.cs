using GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts.Exceptions;
using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using DatabaseBackupEndpointUnderTest = GwiOS.WebUI.Api.Backup.DatabaseBackupEndpoint;

namespace GwiOS.WebUI.Tests.Api.Backup.DatabaseBackupEndpoint;

/// <summary>
/// Covers <c>DatabaseBackupEndpoint.HandleAsync</c>.
/// </summary>
public sealed class HandleAsyncTests
{
    private readonly DatabaseBackupManagerFake _manager = new();
    private readonly LoggerFake<DatabaseBackupEndpointUnderTest> _logger = new();
    private readonly DatabaseBackupEndpointUnderTest _endpoint;

    public HandleAsyncTests()
    {
        _endpoint = new DatabaseBackupEndpointUnderTest(_manager, _logger);
    }

    [Fact]
    public async Task SendsTheBackupAsDownloadUnderItsFileName()
    {
        _manager.FileName = "gwios-backup-2026-10-07_1430.dump";

        IResult result = await _endpoint.HandleAsync(Xunit.TestContext.Current.CancellationToken);

        FileStreamHttpResult file = Assert.IsType<FileStreamHttpResult>(result);
        Assert.Equal("gwios-backup-2026-10-07_1430.dump", file.FileDownloadName);
        Assert.Equal("application/octet-stream", file.ContentType);
    }

    [Fact]
    public async Task SendsTheContentOfTheBackup()
    {
        _manager.Content = [1, 2, 3];

        IResult result = await _endpoint.HandleAsync(Xunit.TestContext.Current.CancellationToken);

        FileStreamHttpResult file = Assert.IsType<FileStreamHttpResult>(result);
        using MemoryStream content = new();
        await file.FileStream.CopyToAsync(content, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal([1, 2, 3], content.ToArray());
    }

    [Fact]
    public async Task RedirectsToTheBackupPageWithTheFailureFlag_WhenTheBackupFails()
    {
        _manager.ExceptionToThrow = new DatabaseBackupFailedException("pg_dump exited with code 1");

        IResult result = await _endpoint.HandleAsync(Xunit.TestContext.Current.CancellationToken);

        RedirectHttpResult redirect = Assert.IsType<RedirectHttpResult>(result);
        Assert.Equal("/admin/datensicherung?fehlgeschlagen=true", redirect.Url);
    }

    [Fact]
    public async Task LogsAWarning_WhenTheBackupFails()
    {
        _manager.ExceptionToThrow = new DatabaseBackupFailedException("pg_dump exited with code 1");

        await _endpoint.HandleAsync(Xunit.TestContext.Current.CancellationToken);

        LogEntry logEntry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Warning, logEntry.LogLevel);
    }
}
