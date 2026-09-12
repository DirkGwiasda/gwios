using GwiOS.WebUI.StatusMessages.Contracts.Models;
using GwiOSStatusBarUnderTest = GwiOS.WebUI.Components.GwiOS.Components.GwiOSStatusBar;

namespace GwiOS.WebUI.Tests.Components.GwiOS.Components.GwiOSStatusBar;

/// <summary>
/// Covers how <c>GwiOSStatusBar</c> follows newly published messages.
/// </summary>
public sealed class MessagesChangedTests : StatusBarProbe
{
    [Fact]
    public void ShowsAMessagePublishedAfterRendering()
    {
        IRenderedComponent<GwiOSStatusBarUnderTest> statusBar = RenderStatusBar();

        StatusMessages.Publish(StatusLevel.Error, "Synchronisation fehlgeschlagen");

        statusBar.WaitForAssertion(
            () => Assert.Equal("Synchronisation fehlgeschlagen", statusBar.Find(".gwios-status-text").TextContent));
    }

    [Fact]
    public void StopsFollowingTheMessages_WhenItIsDisposed()
    {
        IRenderedComponent<GwiOSStatusBarUnderTest> statusBar = RenderStatusBar();
        int subscriberCountWhileRendered = StatusMessages.SubscriberCount;

        statusBar.Instance.Dispose();

        Assert.Equal(1, subscriberCountWhileRendered);
        Assert.Equal(0, StatusMessages.SubscriberCount);
    }
}
