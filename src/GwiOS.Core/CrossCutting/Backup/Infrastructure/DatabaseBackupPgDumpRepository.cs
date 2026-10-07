using System.ComponentModel;
using System.Diagnostics;
using GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts;
using GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts.Exceptions;
using GwiOS.Core.CrossCutting.Backup.Domain.Managers.DatabaseBackupManagement.Contracts.Models;
using GwiOS.Core.CrossCutting.Logging.Contracts;
using Npgsql;

namespace GwiOS.Core.CrossCutting.Backup.Infrastructure;

/// <summary>
/// Dumps the database with <c>pg_dump</c>, which must be on the path in at least the major version of the server.
/// </summary>
internal sealed class DatabaseBackupPgDumpRepository(
    DatabaseBackupOptions options,
    ILogger<DatabaseBackupPgDumpRepository> logger) : IDatabaseBackupRepository
{
    private const string PgDumpExecutable = "pg_dump";

    // The log is not worth restoring and can grow large; its table is still dumped empty, so a restored database
    // has every table the application expects.
    private const string ExcludedTableData = "logging.*";

    private readonly DatabaseBackupOptions _options = options;
    private readonly ILogger<DatabaseBackupPgDumpRepository> _logger = logger;

    public async Task<Stream> CreateDumpAsync(CancellationToken cancellationToken = default)
    {
        // A file instead of streaming the output: only a finished dump tells whether pg_dump succeeded, and a
        // half-sent download would look like a valid backup.
        string dumpPath = Path.GetTempFileName();
        try
        {
            await RunPgDumpAsync(dumpPath, cancellationToken);
            return OpenDump(dumpPath);
        }
        catch
        {
            File.Delete(dumpPath);
            throw;
        }
    }

    private async Task RunPgDumpAsync(string dumpPath, CancellationToken cancellationToken)
    {
        using Process process = StartPgDump(dumpPath);
        Task<string> errorOutput = process.StandardError.ReadToEndAsync(cancellationToken);
        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw;
        }

        string errors = (await errorOutput).Trim();
        if (process.ExitCode != 0)
        {
            throw new DatabaseBackupFailedException($"pg_dump exited with code {process.ExitCode}: {errors}");
        }

        _logger.LogDebug("pg_dump finished.");
    }

    private Process StartPgDump(string dumpPath)
    {
        try
        {
            return Process.Start(CreateStartInfo(dumpPath))
                ?? throw new DatabaseBackupFailedException($"'{PgDumpExecutable}' could not be started.");
        }
        catch (Win32Exception exception)
        {
            throw new DatabaseBackupFailedException(
                $"'{PgDumpExecutable}' could not be started. Is the PostgreSQL client installed?",
                exception);
        }
    }

    // The connection goes through the environment of the process, so the password never shows up in its command
    // line, which every user of the machine could read.
    private ProcessStartInfo CreateStartInfo(string dumpPath)
    {
        NpgsqlConnectionStringBuilder connection = new(_options.ConnectionString);
        ProcessStartInfo startInfo = new(PgDumpExecutable)
        {
            RedirectStandardError = true,
            UseShellExecute = false
        };
        startInfo.ArgumentList.Add("--format=custom");
        startInfo.ArgumentList.Add($"--exclude-table-data={ExcludedTableData}");
        startInfo.ArgumentList.Add($"--file={dumpPath}");
        startInfo.Environment["PGHOST"] = connection.Host;
        startInfo.Environment["PGPORT"] = connection.Port.ToString();
        startInfo.Environment["PGDATABASE"] = connection.Database;
        startInfo.Environment["PGUSER"] = connection.Username;
        startInfo.Environment["PGPASSWORD"] = connection.Password;
        startInfo.Environment["PGSSLMODE"] = ToPgSslMode(connection.SslMode);
        return startInfo;
    }

    private static string ToPgSslMode(SslMode sslMode)
        => sslMode switch
        {
            SslMode.Disable => "disable",
            SslMode.Allow => "allow",
            SslMode.Require => "require",
            SslMode.VerifyCA => "verify-ca",
            SslMode.VerifyFull => "verify-full",
            _ => "prefer"
        };

    private static FileStream OpenDump(string dumpPath)
        => new(
            dumpPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.None,
            bufferSize: 81920,
            FileOptions.Asynchronous | FileOptions.DeleteOnClose);
}
