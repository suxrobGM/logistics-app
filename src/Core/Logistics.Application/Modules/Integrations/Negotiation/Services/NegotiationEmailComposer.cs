using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Logistics.Application.Abstractions.Common;
using Logistics.Application.Abstractions.Email;
using Logistics.Application.Abstractions.Email.Models;
using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Integrations.Negotiation.Services;

/// <summary>
/// The listing, offer, and thread data needed to compose an outbound counter-offer email.
/// <see cref="ReplyToAddress"/> is built by the caller (e.g. from
/// <see cref="IThreadedEmailSender.ReplyDomain"/> plus the negotiation's reply token) - the
/// composer only formats and renders, it never resolves sender configuration itself.
/// </summary>
public record ComposeNegotiationEmailRequest
{
    public const string DefaultCurrency = "USD";

    public required string OriginCity { get; init; }
    public required string OriginState { get; init; }
    public required string DestinationCity { get; init; }
    public required string DestinationState { get; init; }
    public required DateTime PickupDate { get; init; }
    public required string EquipmentType { get; init; }
    public required decimal OfferAmount { get; init; }
    public required string Currency { get; init; }
    public decimal? OfferPerMile { get; init; }
    public required string AgentMessage { get; init; }
    public required string CompanyName { get; init; }
    public string? CompanyMcNumber { get; init; }
    public required string ThreadReference { get; init; }
    public required string ReplyToAddress { get; init; }
    public string? BrokerName { get; init; }

    /// <summary>
    /// Builds the request from the listing and tenant. The preview and the real send both go through
    /// here: the preview's whole promise is that approving it approves the mail that actually goes
    /// out, which two hand-maintained copies of this mapping cannot keep.
    /// </summary>
    public static ComposeNegotiationEmailRequest For(
        LoadBoardListing listing,
        Tenant tenant,
        decimal offerAmount,
        decimal? offerPerMile,
        string agentMessage,
        string replyToAddress)
    {
        return new ComposeNegotiationEmailRequest
        {
            OriginCity = listing.OriginAddress.City,
            OriginState = listing.OriginAddress.State,
            DestinationCity = listing.DestinationAddress.City,
            DestinationState = listing.DestinationAddress.State,
            PickupDate = listing.PickupDateStart ?? listing.ExpiresAt,
            EquipmentType = listing.EquipmentType ?? "Not specified",
            OfferAmount = offerAmount,
            Currency = listing.TotalRate?.Currency ?? DefaultCurrency,
            OfferPerMile = offerPerMile,
            AgentMessage = agentMessage,
            CompanyName = tenant.CompanyName ?? tenant.Name,
            CompanyMcNumber = tenant.McNumber,
            ThreadReference = RateNegotiation.ReferenceFor(listing.Id),
            ReplyToAddress = replyToAddress,
            BrokerName = listing.BrokerName
        };
    }
}

/// <summary>
/// <c>SanitizedMessage</c> is the agent paragraph after sanitization, so the caller can store what
/// the broker actually reads instead of re-running the same cleanup.
/// </summary>
public record ComposedNegotiationEmail(
    string Subject,
    string HtmlBody,
    string SanitizedMessage);

/// <summary>
/// Sole owner of broker-facing negotiation email formatting: sanitizes the agent's free-text
/// paragraph, formats currency/dates, and renders the <c>BrokerCounterOffer</c> template.
/// </summary>
public interface INegotiationEmailComposer : IApplicationService
{
    Task<ComposedNegotiationEmail> ComposeAsync(ComposeNegotiationEmailRequest request, CancellationToken ct = default);
}

internal sealed class NegotiationEmailComposer(IEmailTemplateService emailTemplateService) : INegotiationEmailComposer
{
    private const int MaxMessageLength = 800;
    private const string ReplyInstructions =
        "Reply directly to this email - your response reaches our dispatch team automatically.";

    private static readonly Regex HtmlTagPattern = new("<[^>]*>", RegexOptions.Compiled);
    private static readonly Regex WhitespacePattern = new(@"\s+", RegexOptions.Compiled);

    public async Task<ComposedNegotiationEmail> ComposeAsync(
        ComposeNegotiationEmailRequest request, CancellationToken ct = default)
    {
        var sanitizedMessage = SanitizeMessage(request.AgentMessage);

        var model = new BrokerCounterOfferEmailModel
        {
            BrokerName = request.BrokerName,
            OriginCity = request.OriginCity,
            OriginState = request.OriginState,
            DestinationCity = request.DestinationCity,
            DestinationState = request.DestinationState,
            PickupDate = request.PickupDate.ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture),
            EquipmentType = request.EquipmentType,
            OfferTotal = FormatCurrency(request.OfferAmount, request.Currency),
            OfferPerMile = request.OfferPerMile is { } perMile
                ? FormatCurrency(perMile, request.Currency)
                : null,
            Message = sanitizedMessage,
            CompanyName = request.CompanyName,
            CompanyMcNumber = request.CompanyMcNumber,
            ReferenceNumber = request.ThreadReference,
            ReplyInstructions = ReplyInstructions
        };

        var subject = $"Rate offer: {request.OriginCity}, {request.OriginState} -> " +
                       $"{request.DestinationCity}, {request.DestinationState} - {request.ThreadReference}";
        var htmlBody = await emailTemplateService.RenderAsync("BrokerCounterOffer", model);

        return new ComposedNegotiationEmail(subject, htmlBody, sanitizedMessage);
    }

    private static string FormatCurrency(decimal amount, string currency)
    {
        return currency.Equals("USD", StringComparison.OrdinalIgnoreCase)
            ? amount.ToString("C", CultureInfo.GetCultureInfo("en-US"))
            : $"{amount.ToString("N2", CultureInfo.InvariantCulture)} {currency}";
    }

    internal static string SanitizeMessage(string raw)
    {
        var withoutTags = HtmlTagPattern.Replace(raw, string.Empty);

        // Collapse whitespace (including newlines/tabs) to a single space before stripping
        // control chars, so a line break reads as a word boundary rather than vanishing outright.
        var collapsedWhitespace = WhitespacePattern.Replace(withoutTags, " ");

        var sanitized = new StringBuilder(collapsedWhitespace.Length);
        foreach (var c in collapsedWhitespace)
        {
            if (!char.IsControl(c))
            {
                sanitized.Append(c);
            }
        }

        return NegotiationText.Truncate(
            sanitized.ToString().Trim(), MaxMessageLength, wordBoundary: true);
    }
}
