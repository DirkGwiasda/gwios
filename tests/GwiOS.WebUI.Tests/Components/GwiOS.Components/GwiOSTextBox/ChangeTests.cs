using AngleSharp.Dom;
using GwiOSTextBoxUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSTextBox;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSTextBox;

/// <summary>
/// Covers how <c>GwiOSTextBox</c> reacts when the user leaves the field after changing it.
/// </summary>
public sealed class ChangeTests : BunitContext
{
    [Fact]
    public void ReportsTheTrimmedText()
    {
        string? reportedValue = null;
        IRenderedComponent<GwiOSTextBoxUnderTest> textBox = Render<GwiOSTextBoxUnderTest>(parameters => parameters
            .Add(component => component.ValueChanged, (string value) => reportedValue = value));

        textBox.Find("input").Change("  Einkaufen  ");

        Assert.Equal("Einkaufen", reportedValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FlagsARequiredFieldAsInvalid_WhenItIsLeftEmpty(string text)
    {
        IRenderedComponent<GwiOSTextBoxUnderTest> textBox = Render<GwiOSTextBoxUnderTest>(parameters => parameters
            .Add(component => component.IsRequired, true)
            .Add(component => component.RequiredMessage, "Bitte einen Namen eingeben."));

        textBox.Find("input").Change(text);

        IElement input = textBox.Find("input");
        Assert.True(input.ClassList.Contains("gwios-input--invalid"));
        Assert.Equal("true", input.GetAttribute("aria-invalid"));
        Assert.Equal("Bitte einen Namen eingeben.", input.GetAttribute("title"));
    }

    [Fact]
    public void DoesNotFlagARequiredField_WhenItIsLeftWithText()
    {
        IRenderedComponent<GwiOSTextBoxUnderTest> textBox = Render<GwiOSTextBoxUnderTest>(parameters => parameters
            .Add(component => component.IsRequired, true));

        textBox.Find("input").Change("Anna");

        IElement input = textBox.Find("input");
        Assert.False(input.ClassList.Contains("gwios-input--invalid"));
        Assert.Null(input.GetAttribute("aria-invalid"));
    }

    [Fact]
    public void DoesNotFlagAnOptionalField_WhenItIsLeftEmpty()
    {
        IRenderedComponent<GwiOSTextBoxUnderTest> textBox = Render<GwiOSTextBoxUnderTest>();

        textBox.Find("input").Change(string.Empty);

        Assert.False(textBox.Find("input").ClassList.Contains("gwios-input--invalid"));
    }
}
