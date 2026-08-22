using Logistics.Application.Abstractions.Email;
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
            Currency = ListingCurrency.Of(listing),
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
