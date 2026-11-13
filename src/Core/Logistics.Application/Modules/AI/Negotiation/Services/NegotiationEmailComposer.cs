using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Logistics.Application.Abstractions.Common;
using Logistics.Application.Abstractions.Communications;
using Logistics.Application.Abstractions.Communications.EmailModels;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Modules.AI.Negotiation.Services;

/// <summary>Builds the subject and body of every negotiation email sent to a broker.</summary>
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
            OfferTotal = Money.Format(request.OfferAmount, request.Currency),
            OfferPerDistance = request.OfferPerDistance is { } perDistance
                ? Money.Format(perDistance, request.Currency)
                : null,
            DistanceUnit = request.DistanceUnit,
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
