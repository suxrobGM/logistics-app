using Logistics.Application.Abstractions.Ai;
using Logistics.Application.Abstractions.Documents;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Documents.PdfImport;

/// <summary>
///     Extracts load data from dispatch-sheet PDFs using an LLM. Pulls text with PdfPig and asks the
///     model to return structured JSON; if the PDF has no extractable text (scanned/image-based),
///     falls back to sending the PDF itself to a PDF-native (Anthropic) model. Replaces the former
///     provider-specific regex templates.
/// </summary>
public sealed class LlmPdfDataExtractor(ILlmClient llmClient, ILogger<LlmPdfDataExtractor> logger)
    : IPdfDataExtractor
{
    // Below this many non-whitespace characters the PDF is treated as image-based → vision fallback.
    private const int MinTextLength = 50;
    private const int MaxOutputTokens = 2048;

    public async Task<Result<ExtractedLoadDataDto>> ExtractAsync(
        Stream pdfStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            await pdfStream.CopyToAsync(memoryStream, cancellationToken);
            var pdfBytes = memoryStream.ToArray();

            var request = BuildRequest(pdfBytes, fileName);
            var completion = await llmClient.CompleteAsync(request, cancellationToken);
            if (!completion.IsSuccess)
                return Result<ExtractedLoadDataDto>.Fail(completion.Error ?? "LLM extraction failed.");

            return DispatchSheetResponseParser.Parse(completion.Value!.Text);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error extracting data from PDF {FileName}", fileName);
            return Result<ExtractedLoadDataDto>.Fail($"Error processing PDF: {ex.Message}");
        }
    }

    private LlmCompletionRequest BuildRequest(byte[] pdfBytes, string fileName)
    {
        var pdfText = PdfTextExtractor.Extract(pdfBytes);
        var hasUsableText = pdfText.Count(c => !char.IsWhiteSpace(c)) >= MinTextLength;

        if (hasUsableText)
        {
            logger.LogDebug("Extracted {Length} characters from PDF {FileName}; using text path",
                pdfText.Length, fileName);
            return new LlmCompletionRequest
            {
                SystemPrompt = DispatchSheetPrompt.System,
                UserText = $"Extract the load data from this dispatch sheet:\n\n{pdfText}",
                MaxTokens = MaxOutputTokens
            };
        }

        logger.LogInformation("PDF {FileName} has no extractable text; attaching the PDF for the model to read",
            fileName);
        return new LlmCompletionRequest
        {
            SystemPrompt = DispatchSheetPrompt.System,
            UserText = "Extract the load data from the attached dispatch sheet PDF.",
            Documents = [new LlmInlineDocument("application/pdf", pdfBytes)],
            MaxTokens = MaxOutputTokens
        };
    }
}
