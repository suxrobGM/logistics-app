using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

/// <summary>
/// A broker rate negotiation thread. <c>Messages</c> is populated by the detail query only.
/// The reply token is never exposed - it is the address secret that routes inbound mail.
/// </summary>
public record RateNegotiationDto
{
    public Guid Id { get; set; }
    public Guid LoadBoardListingId { get; set; }
    public Guid? LoadId { get; set; }
    public Guid? ConversationId { get; set; }

    public string Reference { get; set; } = "";

    public string BrokerEmail { get; set; } = "";
    public string? BrokerName { get; set; }
    public string? BrokerMcNumber { get; set; }

    public decimal? FloorRatePerMile { get; set; }
    public Money? FloorTotalRate { get; set; }
    public RateFloorSource FloorSource { get; set; } = RateFloorSource.None;

    public Money? LatestCounterOffer { get; set; }
    public Money? LatestBrokerOffer { get; set; }
    public int RoundCount { get; set; }
    public int MaxRounds { get; set; }

    public RateNegotiationStatus Status { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? CloseReason { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? OriginCity { get; set; }
    public string? OriginState { get; set; }
    public string? DestinationCity { get; set; }
    public string? DestinationState { get; set; }
    public Money? ListingTotalRate { get; set; }

    public List<NegotiationMessageDto> Messages { get; set; } = [];
}
