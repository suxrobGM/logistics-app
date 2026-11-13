using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Security.Cryptography;
using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>An email negotiation with a broker over a load board listing's rate. It closes on acceptance, decline, expiry, or after <see cref="MaxRounds"/> counter-offers.</summary>
public class RateNegotiation : AuditableEntity, ITenantEntity
{
    /// <summary>Outbound counters allowed before the thread must be closed.</summary>
    public const int MaxRounds = 3;

    private static readonly TimeSpan ReplyWindow = TimeSpan.FromHours(48);

    public required Guid LoadBoardListingId { get; set; }
    public virtual LoadBoardListing LoadBoardListing { get; set; } = null!;

    /// <summary>Set once an accepted rate is booked into a load.</summary>
    public Guid? LoadId { get; set; }
    public virtual Load? Load { get; set; }

    /// <summary>The <see cref="AgentConversation"/> the negotiation was opened from.</summary>
    public Guid? ConversationId { get; set; }

    public required string BrokerEmail { get; set; }
    public string? BrokerName { get; set; }
    public string? BrokerMcNumber { get; set; }

    /// <summary>Random token in the thread's reply-to address. Inbound broker mail is matched to the thread only by this token, so it must be unguessable.</summary>
    public required string ReplyToken { get; set; }

    /// <summary>Floor in force when the thread opened - never recomputed mid-negotiation.</summary>
    public decimal? FloorRatePerKm { get; set; }
    public Money? FloorTotalRate { get; set; }
    public RateFloorSource FloorSource { get; set; } = RateFloorSource.None;

    public Money? LatestCounterOffer { get; set; }
    public Money? LatestBrokerOffer { get; set; }
    public int RoundCount { get; set; }

    public RateNegotiationStatus Status { get; set; } = RateNegotiationStatus.AwaitingBroker;

    /// <summary>When the broker's reply window lapses; refreshed by every outbound message.</summary>
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? CloseReason { get; set; }

    public virtual List<NegotiationMessage> Messages { get; } = [];

    /// <summary>Thread reference (NEG-xxxxxxxx), derived from the listing id so a preview built before the thread exists shows the same reference.</summary>
    [NotMapped]
    public string Reference => ReferenceFor(LoadBoardListingId);

    /// <summary>The thread is still live: the broker can reply and the agent can still counter.</summary>
    [NotMapped]
    public bool IsOpen =>
        Status is RateNegotiationStatus.AwaitingBroker or RateNegotiationStatus.BrokerReplied;

    /// <summary>Rounds remain and the thread is open, so another counter may be sent.</summary>
    [NotMapped]
    public bool CanCounter => IsOpen && RoundCount < MaxRounds;

    public static string ReferenceFor(Guid loadBoardListingId) =>
        "NEG-" + loadBoardListingId.ToString("N")[..8].ToUpperInvariant();

    /// <summary>Matches the open (AwaitingBroker or BrokerReplied) negotiation for a listing; the single definition of which statuses count as open.</summary>
    public static Expression<Func<RateNegotiation, bool>> OpenForListing(Guid loadBoardListingId) =>
        n => n.LoadBoardListingId == loadBoardListingId &&
             (n.Status == RateNegotiationStatus.AwaitingBroker ||
              n.Status == RateNegotiationStatus.BrokerReplied);

    /// <summary>Matches every open thread, across all listings.</summary>
    public static Expression<Func<RateNegotiation, bool>> Open() =>
        n => n.Status == RateNegotiationStatus.AwaitingBroker ||
             n.Status == RateNegotiationStatus.BrokerReplied;

    /// <summary>Matches open negotiations past their expiry, including ones the broker has replied to.</summary>
    public static Expression<Func<RateNegotiation, bool>> LapsedAt(DateTime utcNow) =>
        n => (n.Status == RateNegotiationStatus.AwaitingBroker ||
              n.Status == RateNegotiationStatus.BrokerReplied) &&
             n.ExpiresAt != null && n.ExpiresAt < utcNow;

    /// <param name="floor">Floor in force when the thread opens. It is stored on the thread because later checks treat a missing floor as no limit.</param>
    public static RateNegotiation Create(
        Guid loadBoardListingId,
        string brokerEmail,
        RateFloorSnapshot floor,
        string? brokerName = null,
        string? brokerMcNumber = null,
        Guid? conversationId = null)
    {
        return new RateNegotiation
        {
            LoadBoardListingId = loadBoardListingId,
            BrokerEmail = brokerEmail,
            BrokerName = brokerName,
            BrokerMcNumber = brokerMcNumber,
            ConversationId = conversationId,
            ReplyToken = GenerateReplyToken(),
            FloorRatePerKm = floor.MinRatePerKm,
            FloorTotalRate = floor.MinTotalRate,
            FloorSource = floor.Source
        };
    }

    /// <summary>Creates an outbound message for the thread. It is not added to <see cref="Messages"/>, since that would lazy-load every RawBody; the caller reserves the sequence and adds the message with the repository.</summary>
    public NegotiationMessage AddOutboundMessage(
        int sequence,
        string textBody,
        string? subject = null,
        Money? proposedTotalRate = null,
        decimal? proposedRatePerKm = null,
        Guid? agentDecisionId = null)
    {
        var message = new NegotiationMessage
        {
            NegotiationId = Id,
            Sequence = sequence,
            Direction = NegotiationMessageDirection.Outbound,
            Subject = subject,
            TextBody = textBody,
            ProposedTotalRate = proposedTotalRate,
            ProposedRatePerKm = proposedRatePerKm,
            AgentDecisionId = agentDecisionId
        };

        RoundCount++;
        LatestCounterOffer = proposedTotalRate ?? LatestCounterOffer;
        Status = RateNegotiationStatus.AwaitingBroker;
        ExpiresAt = DateTime.UtcNow.Add(ReplyWindow);
        return message;
    }

    /// <summary>Creates an inbound broker message and marks the thread BrokerReplied. Quarantined mail is stored without changing the thread; the caller adds the message with the repository.</summary>
    public NegotiationMessage AddInboundMessage(
        int sequence,
        string textBody,
        string? subject = null,
        string? rawBody = null,
        Money? proposedTotalRate = null,
        decimal? proposedRatePerKm = null,
        string? rfcMessageId = null,
        string? inReplyToMessageId = null,
        bool quarantined = false)
    {
        var message = new NegotiationMessage
        {
            NegotiationId = Id,
            Sequence = sequence,
            Direction = NegotiationMessageDirection.Inbound,
            Subject = subject,
            TextBody = textBody,
            RawBody = rawBody,
            ProposedTotalRate = proposedTotalRate,
            ProposedRatePerKm = proposedRatePerKm,
            RfcMessageId = rfcMessageId,
            InReplyToMessageId = inReplyToMessageId,
            Quarantined = quarantined
        };

        if (!quarantined)
        {
            LatestBrokerOffer = proposedTotalRate ?? LatestBrokerOffer;
            Status = RateNegotiationStatus.BrokerReplied;
        }

        return message;
    }

    public void MarkAccepted(Guid loadId)
    {
        LoadId = loadId;
        Status = RateNegotiationStatus.Accepted;
        ClosedAt = DateTime.UtcNow;
    }

    public void Close(RateNegotiationStatus status, string? reason = null)
    {
        Status = status;
        CloseReason = reason;
        ClosedAt = DateTime.UtcNow;
    }

    /// <summary>Random 128-bit reply token as lowercase hex rather than base64url, because mail systems may change the case of an address's local part.</summary>
    private static string GenerateReplyToken() =>
        Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16));
}
