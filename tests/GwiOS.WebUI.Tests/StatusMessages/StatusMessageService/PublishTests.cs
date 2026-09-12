using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.WebUI.StatusMessages.Contracts.Models;
using GwiOS.WebUI.Tests.TestInfrastructure;
using StatusMessageServiceUnderTest = GwiOS.WebUI.StatusMessages.StatusMessageService;

namespace GwiOS.WebUI.Tests.StatusMessages.StatusMessageService;

/// <summary>
/// Covers <c>StatusMessageService.Publish</c> and the messages it keeps.
/// </summary>
public sealed class PublishTests
{
    private readonly TimeProviderFake _timeProvider = new();
    private readonly LoggerFake<StatusMessageServiceUnderTest> _logger = new();
    private readonly StatusMessageServiceUnderTest _statusMessageService;

    public PublishTests()
    {
        _statusMessageService = new StatusMessageServiceUnderTest(_timeProvider, _logger);
    }

    [Fact]
    public void KeepsNoMessages_BeforeAnythingIsPublished()
    {
        Assert.Empty(_statusMessageService.Messages);
    }

    [Fact]
    public void AddsAMessageWithTheLevelTheTextAndTheCurrentTime()
    {
        _statusMessageService.Publish(StatusLevel.Warning, "Backup verzögert sich");

        StatusMessage message = Assert.Single(_statusMessageService.Messages);
        Assert.Equal(StatusLevel.Warning, message.Level);
        Assert.Equal("Backup verzögert sich", message.Text);
        Assert.Equal(_timeProvider.UtcNow, message.PublishedAt);
    }

    [Fact]
    public void KeepsTheMessagesOldestFirst()
    {
        _statusMessageService.Publish(StatusLevel.Neutral, "Erste");
        _statusMessageService.Publish(StatusLevel.Neutral, "Zweite");

        Assert.Equal(["Erste", "Zweite"], _statusMessageService.Messages.Select(message => message.Text));
    }

    [Fact]
    public void KeepsOnlyTheMostRecentMessages_WhenTheLimitIsExceeded()
    {
        int messageCount = StatusMessageServiceUnderTest.MaxMessageCount + 2;
        for (int number = 1; number <= messageCount; number++)
        {
            _statusMessageService.Publish(StatusLevel.Neutral, $"Meldung {number}");
        }

        Assert.Equal(StatusMessageServiceUnderTest.MaxMessageCount, _statusMessageService.Messages.Count);
        Assert.Equal("Meldung 3", _statusMessageService.Messages[0].Text);
        Assert.Equal($"Meldung {messageCount}", _statusMessageService.Messages[^1].Text);
    }

    [Fact]
    public void RaisesMessagesChangedAfterTheMessageWasAdded()
    {
        int messageCountWhenRaised = -1;
        _statusMessageService.MessagesChanged += () => messageCountWhenRaised = _statusMessageService.Messages.Count;

        _statusMessageService.Publish(StatusLevel.Success, "Erledigt");

        Assert.Equal(1, messageCountWhenRaised);
    }

    [Fact]
    public void LogsTheLevelButNotTheText()
    {
        _statusMessageService.Publish(StatusLevel.Error, "Person „Anna“ fehlt");

        LogEntry logEntry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Debug, logEntry.LogLevel);
        Assert.Equal("Status message published.", logEntry.Message);
        Assert.Equal(nameof(StatusLevel.Error), logEntry.ContextData["StatusLevel"]);
        Assert.DoesNotContain(logEntry.ContextData.Values, value => value.Contains("Anna"));
    }
}
