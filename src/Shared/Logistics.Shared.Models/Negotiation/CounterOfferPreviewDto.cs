namespace Logistics.Shared.Models;

/// <summary>
/// The counter-offer email exactly as it will be sent, rendered from a pending agent decision so
/// the approver reviews the real message rather than a summary of it.
/// </summary>
public record CounterOfferPreviewDto
{
    public string Subject { get; set; } = "";

    /// <summary>
    /// The agent's paragraph after the same sanitization the sent mail gets. Sent instead of the
    /// rendered HTML so the client does not have to strip markup back off to display it.
    /// </summary>
    public string Message { get; set; } = "";

    public string ToEmail { get; set; } = "";
    public string ReplyToAddress { get; set; } = "";
    public decimal ProposedTotalRate { get; set; }
    public decimal? ProposedRatePerMile { get; set; }
    public string Currency { get; set; } = "USD";
}
