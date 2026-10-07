using GwiOS.WebUI.StatusMessages.Contracts;
using GwiOS.WebUI.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MainLayoutUnderTest = GwiOS.WebUI.Components.Layout.MainLayout;

namespace GwiOS.WebUI.Tests.Components.Layout.MainLayout;

/// <summary>
/// Covers how <c>MainLayout</c> tells whether the page is interactive yet, which the end-to-end tests wait for.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    public RenderingTests()
    {
        Services.AddSingleton<IStatusMessageService>(new StatusMessageServiceFake());
        Services.AddSingleton<TimeProvider>(new TimeProviderFake());
    }

    [Fact]
    public void MarksThePageAsInteractive_WhenItRendersInteractively()
    {
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));

        IRenderedComponent<MainLayoutUnderTest> layout = Render<MainLayoutUnderTest>();

        Assert.Equal("true", layout.Find(".gwios-app").GetAttribute("data-interactive"));
    }

    [Fact]
    public void MarksThePageAsNotInteractive_WhilePrerendering()
    {
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));

        IRenderedComponent<MainLayoutUnderTest> layout = Render<MainLayoutUnderTest>();

        Assert.Equal("false", layout.Find(".gwios-app").GetAttribute("data-interactive"));
    }
}
