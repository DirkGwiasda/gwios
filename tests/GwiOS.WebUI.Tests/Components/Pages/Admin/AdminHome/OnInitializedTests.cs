using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using AdminHomeUnderTest = GwiOS.WebUI.Components.Pages.Admin.AdminHome;

namespace GwiOS.WebUI.Tests.Components.Pages.Admin.AdminHome;

/// <summary>
/// Covers the redirect of the admin start page <c>AdminHome</c>.
/// </summary>
public sealed class OnInitializedTests : BunitContext
{
    [Fact]
    public void RedirectsToTheLogging()
    {
        NavigationManager navigationManager = Services.GetRequiredService<NavigationManager>();

        Render<AdminHomeUnderTest>();

        Assert.Equal("http://localhost/admin/logging", navigationManager.Uri);
    }
}
