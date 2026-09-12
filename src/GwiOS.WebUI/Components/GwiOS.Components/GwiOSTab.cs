namespace GwiOS.WebUI.Components.GwiOS.Components;

/// <summary>
/// A tab of a <see cref="GwiOSTabNav"/>, which navigates to a page.
/// </summary>
/// <param name="Text">Text shown on the tab.</param>
/// <param name="Href">Address of the page the tab leads to, relative to the base address of the app.</param>
public record GwiOSTab(string Text, string Href);
