using GwiOS.Core.CrossCutting.Logging.Contracts;
using GwiOS.WebUI.Pdf.Contracts;
using QuestPDF;
using QuestPDF.Drawing;
using QuestPDF.Infrastructure;

namespace GwiOS.WebUI.Pdf;

// QuestPDF keeps its settings and fonts globally; this singleton is the one place that sets them.
internal sealed class QuestPdfConfigurator(ILogger<QuestPdfConfigurator> logger) : IQuestPdfConfigurator
{
    private const string FontResourceSuffix = ".ttf";

    private readonly ILogger<QuestPdfConfigurator> _logger = logger;
    private readonly Lock _lock = new();
    private bool _isConfigured;

    public void EnsureConfigured()
    {
        lock (_lock)
        {
            if (_isConfigured)
            {
                return;
            }

            // Free for individuals and companies below 1 million USD annual revenue; agreed with Dirk (07.10.2026).
            Settings.License = LicenseType.Community;
            // The container has no system fonts; only the embedded ones count, so the PDF looks the same everywhere.
            Settings.UseSystemFonts = false;
            int fontCount = RegisterEmbeddedFonts();
            _isConfigured = true;
            _logger.LogDebug(
                "QuestPDF configured.",
                new Dictionary<string, string> { ["FontCount"] = fontCount.ToString() });
        }
    }

    private static int RegisterEmbeddedFonts()
    {
        System.Reflection.Assembly assembly = typeof(QuestPdfConfigurator).Assembly;
        string[] fontResources = [.. assembly.GetManifestResourceNames()
            .Where(name => name.EndsWith(FontResourceSuffix, StringComparison.OrdinalIgnoreCase))];
        foreach (string fontResource in fontResources)
        {
            using Stream font = assembly.GetManifestResourceStream(fontResource)
                ?? throw new InvalidOperationException($"The embedded font '{fontResource}' could not be read.");
            FontManager.RegisterFontFromStream(font);
        }

        return fontResources.Length;
    }
}
