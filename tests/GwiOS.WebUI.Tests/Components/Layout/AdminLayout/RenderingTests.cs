using AngleSharp.Dom;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using AdminLayoutUnderTest = GwiOS.WebUI.Components.Layout.AdminLayout;

namespace GwiOS.WebUI.Tests.Components.Layout.AdminLayout;

/// <summary>
/// Covers how <c>AdminLayout</c> frames the admin pages.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Fact]
    public void ShowsTheAdminTabsAboveThePage()
    {
        RenderFragment body = builder => BuildBody(builder);

        IRenderedComponent<AdminLayoutUnderTest> layout = Render<AdminLayoutUnderTest>(parameters => parameters
            .Add(component => component.Body, body));

        IReadOnlyList<IElement> tabs = layout.FindAll(".gwios-tab-nav a");
        Assert.Equal(["Logging", "Benutzerverwaltung"], tabs.Select(tab => tab.TextContent));
        Assert.Equal(["admin/logging", "admin/benutzer"], tabs.Select(tab => tab.GetAttribute("href")));
        Assert.Equal("Seite", layout.Find("p.page").TextContent);
    }

    private static void BuildBody(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "p");
        builder.AddAttribute(1, "class", "page");
        builder.AddContent(2, "Seite");
        builder.CloseElement();
    }
}
