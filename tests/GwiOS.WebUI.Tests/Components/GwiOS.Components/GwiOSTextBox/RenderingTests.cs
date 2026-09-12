using AngleSharp.Dom;
using GwiOSTextBoxUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSTextBox;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSTextBox;

/// <summary>
/// Covers how <c>GwiOSTextBox</c> renders its parameters.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Fact]
    public void RendersATextInputWithTheValueAndThePlaceholder()
    {
        IRenderedComponent<GwiOSTextBoxUnderTest> textBox = Render<GwiOSTextBoxUnderTest>(parameters => parameters
            .Add(component => component.Value, "Einkaufen")
            .Add(component => component.Placeholder, "Titel…"));

        IElement input = textBox.Find("input");
        Assert.Equal("text", input.GetAttribute("type"));
        Assert.Equal("Einkaufen", input.GetAttribute("value"));
        Assert.Equal("Titel…", input.GetAttribute("placeholder"));
    }

    [Fact]
    public void UsesThePlaceholderAsAccessibleName_WhenNoAriaLabelIsSet()
    {
        IRenderedComponent<GwiOSTextBoxUnderTest> textBox = Render<GwiOSTextBoxUnderTest>(parameters => parameters
            .Add(component => component.Placeholder, "Titel…"));

        Assert.Equal("Titel…", textBox.Find("input").GetAttribute("aria-label"));
    }

    [Fact]
    public void UsesTheAriaLabelAsAccessibleName_WhenItIsSet()
    {
        IRenderedComponent<GwiOSTextBoxUnderTest> textBox = Render<GwiOSTextBoxUnderTest>(parameters => parameters
            .Add(component => component.Placeholder, "Titel…")
            .Add(component => component.AriaLabel, "Titel des ToDos"));

        Assert.Equal("Titel des ToDos", textBox.Find("input").GetAttribute("aria-label"));
    }

    [Fact]
    public void LimitsTheLength_WhenAMaximumIsSet()
    {
        IRenderedComponent<GwiOSTextBoxUnderTest> textBox = Render<GwiOSTextBoxUnderTest>(parameters => parameters
            .Add(component => component.MaxLength, 30));

        Assert.Equal("30", textBox.Find("input").GetAttribute("maxlength"));
    }

    [Fact]
    public void AddsTheCssClassesAndPassesOnFurtherAttributes()
    {
        IRenderedComponent<GwiOSTextBoxUnderTest> textBox = Render<GwiOSTextBoxUnderTest>(parameters => parameters
            .Add(component => component.CssClass, "wide")
            .AddUnmatched("id", "title-input"));

        IElement input = textBox.Find("input");
        Assert.True(input.ClassList.Contains("gwios-input"));
        Assert.True(input.ClassList.Contains("wide"));
        Assert.Equal("title-input", input.GetAttribute("id"));
    }
}
