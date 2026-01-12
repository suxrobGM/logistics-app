using System.Text;

using Logistics.Application.Services.PdfImport;
using Logistics.Infrastructure.Services.PdfImport.Templates;
using Logistics.Shared.Models;

using Microsoft.Extensions.Logging;

using UglyToad.PdfPig;

namespace Logistics.Infrastructure.Services.PdfImport;

/// <summary>
///     PDF data extractor that uses template-based regex parsing.
///     Automatically detects the PDF format and uses the appropriate template.
/// </summary>
public sealed class TemplateBasedDataExtractor(ILogger<TemplateBasedDataExtractor> logger) : IPdfDataExtractor
{
    private readonly IReadOnlyList<IDispatchSheetTemplate> _templates =
    [
        new SuperDispatchTemplate(),
        new ShipCarsTemplate(),
        new CentralDispatchTemplate()
    ];

    // Register all available templates

    public async Task<Result<ExtractedLoadData>> ExtractAsync(
        Stream pdfStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Read the PDF stream into memory
            using var memoryStream = new MemoryStream();
            await pdfStream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            // Extract text from PDF using PdfPig
            var pdfText = ExtractTextFromPdf(memoryStream);

            if (string.IsNullOrWhiteSpace(pdfText))
            {
                return Result<ExtractedLoadData>.Fail(
                    "Unable to extract text from PDF. The file may be image-based or corrupted.");
            }

            logger.LogDebug("Extracted {Length} characters from PDF {FileName}", pdfText.Length, fileName);

            // Find a template that can parse this PDF
            var template = FindMatchingTemplate(pdfText);

            if (template is null)
            {
                var supportedFormats = string.Join(", ", _templates.Select(t => t.TemplateName));
                return Result<ExtractedLoadData>.Fail(
                    $"Unsupported dispatch sheet format. Supported formats: {supportedFormats}");
            }

            logger.LogInformation("Using template {TemplateName} to parse PDF {FileName}",
                template.TemplateName, fileName);

            // Extract data using the template
            var extractedData = template.Extract(pdfText);

            // Validate required fields
            var validationResult = ValidateExtractedData(extractedData);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            return Result<ExtractedLoadData>.Ok(extractedData);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extracting data from PDF {FileName}", fileName);
            return Result<ExtractedLoadData>.Fail($"Error processing PDF: {ex.Message}");
        }
    }

    private static string ExtractTextFromPdf(MemoryStream pdfStream)
    {
        using var document = PdfDocument.Open(pdfStream);

        var textBuilder = new StringBuilder();

        foreach (var page in document.GetPages())
        {
            textBuilder.AppendLine(page.Text);
        }

        return textBuilder.ToString();
    }

    private IDispatchSheetTemplate? FindMatchingTemplate(string pdfText)
    {
        foreach (var template in _templates)
        {
            if (template.CanParse(pdfText))
            {
                return template;
            }
        }

        return null;
    }

    private static Result<ExtractedLoadData> ValidateExtractedData(ExtractedLoadData data)
    {
        var errors = new List<string>();

        // Check for required fields
        if (data.OriginAddress is null ||
            string.IsNullOrWhiteSpace(data.OriginAddress.Line1) ||
            string.IsNullOrWhiteSpace(data.OriginAddress.City) ||
            string.IsNullOrWhiteSpace(data.OriginAddress.State))
        {
            errors.Add("Origin address could not be extracted");
        }

        if (data.DestinationAddress is null ||
            string.IsNullOrWhiteSpace(data.DestinationAddress.Line1) ||
            string.IsNullOrWhiteSpace(data.DestinationAddress.City) ||
            string.IsNullOrWhiteSpace(data.DestinationAddress.State))
        {
            errors.Add("Destination address could not be extracted");
        }

        if (!data.PaymentAmount.HasValue || data.PaymentAmount.Value <= 0)
        {
            errors.Add("Payment amount could not be extracted");
        }

        if (errors.Count > 0)
        {
            return Result<ExtractedLoadData>.Fail(
                $"Missing required fields: {string.Join(", ", errors)}");
        }

        return Result<ExtractedLoadData>.Ok(data);
    }
}
