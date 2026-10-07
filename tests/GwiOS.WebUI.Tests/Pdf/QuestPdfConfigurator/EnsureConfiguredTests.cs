using GwiOS.Core.CrossCutting.Logging.Domain.Contracts.Models;
using GwiOS.WebUI.Tests.TestInfrastructure;
using QuestPDF;
using QuestPDF.Infrastructure;
using QuestPdfConfiguratorUnderTest = GwiOS.WebUI.Pdf.QuestPdfConfigurator;

namespace GwiOS.WebUI.Tests.Pdf.QuestPdfConfigurator;

/// <summary>
/// Covers <c>QuestPdfConfigurator.EnsureConfigured</c>. QuestPDF keeps its settings globally, so these tests only
/// check settings every configuration leaves the same.
/// </summary>
public sealed class EnsureConfiguredTests
{
    private readonly LoggerFake<QuestPdfConfiguratorUnderTest> _logger = new();
    private readonly QuestPdfConfiguratorUnderTest _configurator;

    public EnsureConfiguredTests()
    {
        _configurator = new QuestPdfConfiguratorUnderTest(_logger);
    }

    [Fact]
    public void UsesTheCommunityLicenseAndNoSystemFonts()
    {
        _configurator.EnsureConfigured();

        Assert.Equal(LicenseType.Community, Settings.License);
        Assert.False(Settings.UseSystemFonts);
    }

    [Fact]
    public void RegistersTheThreeEmbeddedInterFonts()
    {
        _configurator.EnsureConfigured();

        LogEntry logEntry = Assert.Single(_logger.Entries);
        Assert.Equal("3", logEntry.ContextData["FontCount"]);
    }

    [Fact]
    public void ConfiguresOnlyOnce_WhenCalledRepeatedly()
    {
        _configurator.EnsureConfigured();
        _configurator.EnsureConfigured();

        Assert.Single(_logger.Entries);
    }
}
