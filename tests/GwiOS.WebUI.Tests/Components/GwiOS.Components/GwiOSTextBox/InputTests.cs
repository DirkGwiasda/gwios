using GwiOSTextBoxUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSTextBox;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSTextBox;

/// <summary>
/// Covers how <c>GwiOSTextBox</c> reacts while the user types.
/// </summary>
public sealed class InputTests : BunitContext
{
    [Fact]
    public void ReportsEveryKeystrokeUntrimmed()
    {
        List<string> reportedValues = [];
        IRenderedComponent<GwiOSTextBoxUnderTest> textBox = Render<GwiOSTextBoxUnderTest>(parameters => parameters
            .Add(component => component.ValueChanged, (string value) => reportedValues.Add(value)));

        textBox.Find("input").Input("E");
        textBox.Find("input").Input("Ei ");

        Assert.Equal(["E", "Ei "], reportedValues);
    }

    [Fact]
    public void DoesNotFlagARequiredField_WhileTheUserIsStillTyping()
    {
        IRenderedComponent<GwiOSTextBoxUnderTest> textBox = Render<GwiOSTextBoxUnderTest>(parameters => parameters
            .Add(component => component.IsRequired, true));

        textBox.Find("input").Input(string.Empty);

        Assert.False(textBox.Find("input").ClassList.Contains("gwios-input--invalid"));
    }
}
