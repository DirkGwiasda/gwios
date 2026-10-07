namespace GwiOS.WebUI.Api.Backup.Contracts;

/// <summary>
/// Handles <c>GET /api/backup</c>: creates a backup of the GwiOS database and sends it as a file download, so it can
/// be kept outside the server.
/// </summary>
public interface IDatabaseBackupEndpoint
{
    /// <summary>
    /// Creates the backup and returns it as a download. If the backup fails, redirects to the page
    /// <see cref="FailurePageUrl"/> instead, which explains the failure, because the request comes from a link in the
    /// browser and an error status would leave the user on a blank page.
    /// </summary>
    Task<IResult> HandleAsync(CancellationToken cancellationToken);

    /// <summary>
    /// The address the endpoint redirects to if the backup fails.
    /// </summary>
    static string FailurePageUrl
        => "/admin/datensicherung?fehlgeschlagen=true";
}
