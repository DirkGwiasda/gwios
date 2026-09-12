using AngleSharp.Dom;
using GwiOS.WebUI.Components.GwiOS.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using GwiOSTabNavUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSTabNav;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSTabNav;

/// <summary>
/// Covers how <c>GwiOSTabNav</c> renders its tabs and marks the active one.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    private readonly GwiOSTab[] _tabs = [new("Logging", "admin/logging"), new("Benutzerverwaltung", "admin/benutzer")];

    [Fact]
    public void RendersALinkPerTabInOrder()
    {
        IRenderedComponent<GwiOSTabNavUnderTest> tabNav = Render<GwiOSTabNavUnderTest>(parameters => parameters
            .Add(component => component.Tabs, _tabs));

        IReadOnlyList<IElement> links = tabNav.FindAll("a");
        Assert.Equal(["Logging", "Benutzerverwaltung"], links.Select(link => link.TextContent));
        Assert.Equal(["admin/logging", "admin/benutzer"], links.Select(link => link.GetAttribute("href")));
    }

    [Fact]
    public void MarksTheTabOfTheCurrentPageAsActive()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("admin/benutzer");

        IRenderedComponent<GwiOSTabNavUnderTest> tabNav = Render<GwiOSTabNavUnderTest>(parameters => parameters
            .Add(component => component.Tabs, _tabs));

        IElement activeLink = tabNav.Find("a.active");
        Assert.Equal("Benutzerverwaltung", activeLink.TextContent);
    }

    [Fact]
    public void UsesTheAriaLabelAsAccessibleName()
    {
        IRenderedComponent<GwiOSTabNavUnderTest> tabNav = Render<GwiOSTabNavUnderTest>(parameters => parameters
            .Add(component => component.Tabs, _tabs)
            .Add(component => component.AriaLabel, "Adminbereiche"));

        Assert.Equal("Adminbereiche", tabNav.Find("nav").GetAttribute("aria-label"));
    }
}
