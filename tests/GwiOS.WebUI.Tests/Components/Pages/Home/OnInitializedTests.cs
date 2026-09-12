using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using HomeUnderTest = GwiOS.WebUI.Components.Pages.Home;

namespace GwiOS.WebUI.Tests.Components.Pages.Home;

/// <summary>
/// Covers the redirect of the start page <c>Home</c>.
/// </summary>
public sealed class OnInitializedTests : BunitContext
{
    [Fact]
    public void RedirectsToTheToDoOverview()
    {
        NavigationManager navigationManager = Services.GetRequiredService<NavigationManager>();

        Render<HomeUnderTest>();

        Assert.Equal("http://localhost/todos", navigationManager.Uri);
    }
}
