namespace GwiOS.WebUI.Pdf.Contracts;

/// <summary>
/// Prepares the PDF library QuestPDF once per application: its license and the fonts the documents use.
/// </summary>
public interface IQuestPdfConfigurator
{
    /// <summary>
    /// Configures QuestPDF if that has not happened yet; later calls do nothing.
    /// </summary>
    void EnsureConfigured();
}
