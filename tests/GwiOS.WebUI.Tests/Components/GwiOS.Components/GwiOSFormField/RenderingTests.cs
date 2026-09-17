using AngleSharp.Dom;
using GwiOSFormFieldUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSFormField;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSFormField;

/// <summary>
/// Covers how <c>GwiOSFormField</c> arranges label, input, hint and error.
/// </summary>
public sealed class RenderingTests : BunitContext
{
    [Fact]
    public void ShowsTheLabelForTheInputAndTheInput()
    {
        IRenderedComponent<GwiOSFormFieldUnderTest> field = Render<GwiOSFormFieldUnderTest>(parameters => parameters
            .Add(component => component.Label, "Name")
            .Add(component => component.InputId, "name-input")
            .AddChildContent("<input id=\"name-input\" />"));

        IElement label = field.Find("label");
        Assert.Equal("Name", label.TextContent.Trim());
        Assert.Equal("name-input", label.GetAttribute("for"));
        Assert.Single(field.FindAll("input#name-input"));
    }

    [Fact]
    public void ShowsNeitherAsteriskNorHintNorError_ByDefault()
    {
        IRenderedComponent<GwiOSFormFieldUnderTest> field = Render<GwiOSFormFieldUnderTest>(parameters => parameters
            .Add(component => component.Label, "Name")
            .Add(component => component.InputId, "name-input"));

        Assert.Empty(field.FindAll(".gwios-form-field-required"));
        Assert.Empty(field.FindAll(".gwios-form-field-hint"));
        Assert.Empty(field.FindAll(".gwios-form-field-error"));
    }

    [Fact]
    public void MarksTheLabelWithAnAsterisk_WhenTheFieldIsRequired()
    {
        IRenderedComponent<GwiOSFormFieldUnderTest> field = Render<GwiOSFormFieldUnderTest>(parameters => parameters
            .Add(component => component.Label, "Name")
            .Add(component => component.InputId, "name-input")
            .Add(component => component.IsRequired, true));

        Assert.Equal("*", field.Find("label .gwios-form-field-required").TextContent);
    }

    [Fact]
    public void ShowsTheHintAndTheError_WhenTheyAreSet()
    {
        IRenderedComponent<GwiOSFormFieldUnderTest> field = Render<GwiOSFormFieldUnderTest>(parameters => parameters
            .Add(component => component.Label, "Kurzname")
            .Add(component => component.InputId, "short-name-input")
            .Add(component => component.Hint, "Aus dem Kurznamen entsteht das Kürzel.")
            .Add(component => component.ErrorMessage, "Dieser Kurzname ist bereits vergeben"));

        Assert.Equal("Aus dem Kurznamen entsteht das Kürzel.", field.Find(".gwios-form-field-hint").TextContent);
        IElement error = field.Find(".gwios-form-field-error");
        Assert.Equal("Dieser Kurzname ist bereits vergeben", error.TextContent);
        Assert.Equal("alert", error.GetAttribute("role"));
    }
}
