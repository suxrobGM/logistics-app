using System.Text;
using UglyToad.PdfPig;

namespace Logistics.Infrastructure.Documents.PdfImport;

/// <summary>
///     Extracts the concatenated text of a PDF using PdfPig. Returns near-empty text for
///     scanned/image-based PDFs, which signals the caller to use a vision path.
/// </summary>
internal static class PdfTextExtractor
{
    public static string Extract(byte[] pdfBytes)
    {
        using var stream = new MemoryStream(pdfBytes);
        using var document = PdfDocument.Open(stream);

        var builder = new StringBuilder();
        foreach (var page in document.GetPages())
            builder.AppendLine(page.Text);

        return builder.ToString();
    }
}
