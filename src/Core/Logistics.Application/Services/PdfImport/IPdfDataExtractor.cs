using Logistics.Shared.Models;

namespace Logistics.Application.Services.PdfImport;

/// <summary>
/// Service for extracting load data from PDF files.
/// </summary>
public interface IPdfDataExtractor
{
    /// <summary>
    /// Extracts load data from a PDF file stream.
    /// </summary>
    /// <param name="pdfStream">The PDF file stream.</param>
    /// <param name="fileName">The original file name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Extracted load data or error.</returns>
    Task<Result<ExtractedLoadData>> ExtractAsync(Stream pdfStream, string fileName, CancellationToken ct = default);
}
