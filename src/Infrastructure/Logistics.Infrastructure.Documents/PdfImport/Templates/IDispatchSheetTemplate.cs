using Logistics.Shared.Models;

namespace Logistics.Infrastructure.Services.PdfImport.Templates;

/// <summary>
/// Interface for dispatch sheet PDF templates.
/// Each broker/platform has its own PDF format that requires a specific parser.
/// </summary>
public interface IDispatchSheetTemplate
{
    /// <summary>
    /// The name of this template (e.g., "Super Dispatch", "Ship.Cars").
    /// </summary>
    string TemplateName { get; }

    /// <summary>
    /// Determines if this template can parse the given PDF text.
    /// </summary>
    /// <param name="pdfText">The extracted text content from the PDF.</param>
    /// <returns>True if this template recognizes and can parse the PDF format.</returns>
    bool CanParse(string pdfText);

    /// <summary>
    /// Extracts load data from the PDF text.
    /// </summary>
    /// <param name="pdfText">The extracted text content from the PDF.</param>
    /// <returns>The extracted load data.</returns>
    ExtractedLoadDataDto Extract(string pdfText);
}
