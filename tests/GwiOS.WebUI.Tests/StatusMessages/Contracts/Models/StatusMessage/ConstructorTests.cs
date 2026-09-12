using GwiOS.WebUI.StatusMessages.Contracts.Models;
using StatusMessageUnderTest = GwiOS.WebUI.StatusMessages.Contracts.Models.StatusMessage;

namespace GwiOS.WebUI.Tests.StatusMessages.Contracts.Models.StatusMessage;

/// <summary>
/// Covers the construction of <c>StatusMessage</c> and the defaults it assigns.
/// </summary>
public sealed class ConstructorTests
{
    private readonly DateTimeOffset _publishedAt = new(2026, 7, 14, 10, 30, 0, TimeSpan.Zero);

    [Fact]
    public void AssignsAVersion7Id()
    {
        StatusMessageUnderTest message = new(StatusLevel.Neutral, "Text", _publishedAt);

        Assert.Equal(7, message.Id.Version);
    }

    [Fact]
    public void AssignsADifferentIdToEachMessage()
    {
        StatusMessageUnderTest firstMessage = new(StatusLevel.Neutral, "Text", _publishedAt);
        StatusMessageUnderTest secondMessage = new(StatusLevel.Neutral, "Text", _publishedAt);

        Assert.NotEqual(firstMessage.Id, secondMessage.Id);
    }
}
