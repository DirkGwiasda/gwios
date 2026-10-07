using AngleSharp.Dom;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using DatabaseBackupUnderTest = GwiOS.WebUI.Components.Pages.Admin.DatabaseBackup;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.DatabaseBackup;

/// <summary>
/// Covers how the page <c>DatabaseBackup</c> offers the download and reports a failed backup.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Fact]
    public void LinksToTheBackupEndpointAsDownload()
    {
        IRenderedComponent<DatabaseBackupUnderTest> page = Render<DatabaseBackupUnderTest>();

        IElement link = page.Find("a.gwios-button");
        Assert.Equal("api/backup", link.GetAttribute("href"));
        Assert.True(link.HasAttribute("download"));
        Assert.Equal("Sicherung herunterladen", link.TextContent);
    }

    [Fact]
    public void ShowsNoError_WhenTheBackupDidNotFail()
    {
        IRenderedComponent<DatabaseBackupUnderTest> page = Render<DatabaseBackupUnderTest>();

        Assert.Empty(page.FindAll("[role=alert]"));
    }

    [Fact]
    public void ShowsAnError_WhenTheEndpointRedirectedBecauseTheBackupFailed()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("admin/datensicherung?fehlgeschlagen=true");

        IRenderedComponent<DatabaseBackupUnderTest> page = Render<DatabaseBackupUnderTest>();

        Assert.Contains("konnte nicht erstellt werden", page.Find("[role=alert]").TextContent);
    }
}
