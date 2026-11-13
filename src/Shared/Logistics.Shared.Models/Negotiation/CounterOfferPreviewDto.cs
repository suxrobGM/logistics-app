namespace Logistics.Shared.Models;

/// <summary>The counter-offer email a pending agent decision would send, rendered from that decision so the approver reviews the exact message.</summary>
public record CounterOfferPreviewDto
{
    public string Subject { get; set; } = "";

    /// <summary>Message body as sanitized plain text, not HTML, so the client can display it without stripping markup.</summary>
    public string Message { get; set; } = "";

    public string ToEmail { get; set; } = "";
    public string ReplyToAddress { get; set; } = "";
    public decimal ProposedTotalRate { get; set; }
    public decimal? ProposedRatePerKm { get; set; }
    public string Currency { get; set; } = "USD";
}
