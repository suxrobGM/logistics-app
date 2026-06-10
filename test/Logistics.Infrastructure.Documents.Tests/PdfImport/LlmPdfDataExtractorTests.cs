using System.Runtime.CompilerServices;
using Logistics.Application.Abstractions.Ai;
using Logistics.Infrastructure.Services.PdfImport;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Xunit;

namespace Logistics.Infrastructure.Documents.Tests.PdfImport;

public class LlmPdfDataExtractorTests
{
    private const string ValidJson =
        """
        {
          "orderId": "ORD-123",
          "vehicleYear": 2020,
          "vehicleMake": "Toyota",
          "vehicleModel": "Camry",
          "vehicleVin": "1HGBH41JXMN109186",
          "vehicleType": "Sedan",
          "originAddress": { "line1": "100 Pickup St", "city": "Dallas", "state": "TX", "zipCode": "75201", "country": "USA" },
          "destinationAddress": { "line1": "200 Dropoff Ave", "city": "Phoenix", "state": "AZ", "zipCode": "85001", "country": "USA" },
          "pickupDate": "2026-06-10",
          "deliveryDate": "2026-06-12",
          "paymentAmount": "$1,250.00",
          "shipperName": "Acme Brokerage"
        }
        """;

    private readonly ILlmClient llmClient = Substitute.For<ILlmClient>();
    private readonly LlmPdfDataExtractor sut;

    static LlmPdfDataExtractorTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public LlmPdfDataExtractorTests()
    {
        sut = new LlmPdfDataExtractor(llmClient, NullLogger<LlmPdfDataExtractor>.Instance);
    }

    [Fact]
    public async Task ExtractAsync_ValidJson_MapsAllFields()
    {
        SetupCompletion(ValidJson);

        var result = await sut.ExtractAsync(TextPdf("This is a dispatch sheet with enough text for the text path."), "load.pdf");

        Assert.True(result.IsSuccess);
        var dto = result.Value!;
        Assert.Equal("ORD-123", dto.OrderId);
        Assert.Equal(2020, dto.VehicleYear);
        Assert.Equal("Dallas", dto.OriginAddress!.City);
        Assert.Equal("Phoenix", dto.DestinationAddress!.City);
        Assert.Equal(1250.00m, dto.PaymentAmount);
        Assert.Equal(new DateTime(2026, 6, 10), dto.PickupDate!.Value.Date);
        Assert.Equal("AI", dto.SourceTemplate);
    }

    [Fact]
    public async Task ExtractAsync_DigitalPdf_UsesTextPathWithoutDocuments()
    {
        var captured = SetupCompletion(ValidJson);

        await sut.ExtractAsync(TextPdf("A digital dispatch sheet with plenty of selectable text content."), "load.pdf");

        Assert.NotNull(captured.Value);
        Assert.Empty(captured.Value!.Documents);
    }

    [Fact]
    public async Task ExtractAsync_ImageOnlyPdf_AttachesPdfForModelToRead()
    {
        var captured = SetupCompletion(ValidJson);

        await sut.ExtractAsync(ImageOnlyPdf(), "scan.pdf");

        Assert.NotNull(captured.Value);
        var document = Assert.Single(captured.Value!.Documents);
        Assert.Equal("application/pdf", document.MediaType);
    }

    [Fact]
    public async Task ExtractAsync_ResponseWrappedInMarkdownFence_StillParses()
    {
        SetupCompletion($"```json\n{ValidJson}\n```");

        var result = await sut.ExtractAsync(TextPdf("Dispatch sheet text long enough for the text path to run."), "load.pdf");

        Assert.True(result.IsSuccess);
        Assert.Equal("ORD-123", result.Value!.OrderId);
    }

    [Fact]
    public async Task ExtractAsync_NonJsonResponse_ReturnsFailure()
    {
        SetupCompletion("Sorry, this does not look like a dispatch sheet.");

        var result = await sut.ExtractAsync(TextPdf("Some unrelated document text that is sufficiently long here."), "load.pdf");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task ExtractAsync_MissingRequiredFields_ReturnsFailure()
    {
        SetupCompletion("""{ "orderId": "ORD-1", "paymentAmount": "500" }""");

        var result = await sut.ExtractAsync(TextPdf("Dispatch sheet text long enough for the text path to run here."), "load.pdf");

        Assert.False(result.IsSuccess);
        Assert.Contains("Origin address", result.Error);
    }

    [Fact]
    public async Task ExtractAsync_LlmClientFails_PropagatesError()
    {
        llmClient.CompleteAsync(Arg.Any<LlmCompletionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<LlmCompletionResult>.Fail("LLM API key for provider 'Anthropic' is not configured."));

        var result = await sut.ExtractAsync(TextPdf("A dispatch sheet with enough extractable text to take the path."), "load.pdf");

        Assert.False(result.IsSuccess);
        Assert.Contains("not configured", result.Error);
    }

    /// <summary>Sets up the mock and returns a box capturing the request the extractor sends.</summary>
    private StrongBox<LlmCompletionRequest?> SetupCompletion(string responseText)
    {
        var captured = new StrongBox<LlmCompletionRequest?>(null);
        llmClient
            .CompleteAsync(Arg.Do<LlmCompletionRequest>(r => captured.Value = r), Arg.Any<CancellationToken>())
            .Returns(Result<LlmCompletionResult>.Ok(
                new LlmCompletionResult(responseText, "claude-haiku-4-5", 10, 20, 0.0001m)));
        return captured;
    }

    private static MemoryStream TextPdf(string text)
    {
        var bytes = Document.Create(c => c.Page(p =>
        {
            p.Margin(20);
            p.Content().Text(text);
        })).GeneratePdf();
        return new MemoryStream(bytes);
    }

    private static MemoryStream ImageOnlyPdf()
    {
        // A page with no text content — PdfPig extracts nothing, forcing the vision fallback.
        var bytes = Document.Create(c => c.Page(p =>
        {
            p.Margin(20);
            p.Content().Background(Colors.Grey.Medium);
        })).GeneratePdf();
        return new MemoryStream(bytes);
    }
}
