using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Security.Cryptography;
using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// An email thread with a broker haggling over the rate of a load board listing. The agent counters,
/// the broker replies to the <see cref="ReplyToken"/> address, and the thread closes on acceptance,
/// decline, expiry, or when the round budget runs out.
/// </summary>
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

    /// <summary>
    /// Opaque token embedded in the reply-to address; the only link from inbound mail back to this
    /// thread, so it must stay unguessable.
    /// </summary>
    public required string ReplyToken { get; set; }

    /// <summary>Floor in force when the thread opened - never recomputed mid-negotiation.</summary>
    public decimal? FloorRatePerMile { get; set; }
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

    /// <summary>
    /// Highest sequence handed out so far. Held here so allocating the next one never reads the
    /// Messages navigation, which lazy-loads every stored RawBody.
    /// </summary>
    public int LastSequence { get; private set; }

    /// <summary>
    /// Reference quoted in the email subject. Derived from the listing rather than the thread so a
    /// preview rendered before the thread exists carries the same reference the sent mail will.
    /// </summary>
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

    /// <summary>
    /// Matches the single open thread on a listing. Kept here rather than inlined at each call site
    /// so a new open status cannot be added without every query picking it up.
    /// </summary>
    public static Expression<Func<RateNegotiation, bool>> OpenForListing(Guid loadBoardListingId) =>
        n => n.LoadBoardListingId == loadBoardListingId &&
             (n.Status == RateNegotiationStatus.AwaitingBroker ||
              n.Status == RateNegotiationStatus.BrokerReplied);

    /// <summary>Matches every open thread, across all listings.</summary>
    public static Expression<Func<RateNegotiation, bool>> Open() =>
        n => n.Status == RateNegotiationStatus.AwaitingBroker ||
             n.Status == RateNegotiationStatus.BrokerReplied;

    /// <summary>
    /// Matches every open thread whose reply window has already lapsed. A thread the broker replied
    /// to is still open and still lapses, so the sweep must see it too.
    /// </summary>
    public static Expression<Func<RateNegotiation, bool>> LapsedAt(DateTime utcNow) =>
        n => (n.Status == RateNegotiationStatus.AwaitingBroker ||
              n.Status == RateNegotiationStatus.BrokerReplied) &&
             n.ExpiresAt != null && n.ExpiresAt < utcNow;

    /// <param name="floor">
    /// Snapshotted onto the thread here rather than assigned by the caller: every downstream check
    /// reads a missing floor as "no limit", so a floorless thread must be unrepresentable.
    /// </param>
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
            FloorRatePerMile = floor.MinRatePerMile,
            FloorTotalRate = floor.MinTotalRate,
            FloorSource = floor.Source
        };
    }

    /// <summary>
    /// Records a counter sent to the broker: consumes a round and restarts the reply window. The
    /// returned row is not added to <see cref="Messages"/> - touching that navigation lazy-loads
    /// every stored RawBody. Callers MUST register it via repository AddAsync instead (pre-generated
    /// ids make a collection-only add save as an UPDATE and fail anyway).
    /// </summary>
    public NegotiationMessage AddOutboundMessage(
        string textBody,
        string? subject = null,
        Money? proposedTotalRate = null,
        decimal? proposedRatePerMile = null,
        Guid? agentDecisionId = null)
    {
        var message = new NegotiationMessage
        {
            NegotiationId = Id,
            Sequence = NextSequence(),
            Direction = NegotiationMessageDirection.Outbound,
            Subject = subject,
            TextBody = textBody,
            ProposedTotalRate = proposedTotalRate,
            ProposedRatePerMile = proposedRatePerMile,
            AgentDecisionId = agentDecisionId
        };

        RoundCount++;
        LatestCounterOffer = proposedTotalRate ?? LatestCounterOffer;
        Status = RateNegotiationStatus.AwaitingBroker;
        ExpiresAt = DateTime.UtcNow.Add(ReplyWindow);
        return message;
    }

    /// <summary>
    /// Records a broker reply. A quarantined message is stored for audit but moves nothing. The
    /// returned row is not added to <see cref="Messages"/> - touching that navigation lazy-loads
    /// every stored RawBody. Callers MUST register it via repository AddAsync instead (pre-generated
    /// ids make a collection-only add save as an UPDATE and fail anyway).
    /// </summary>
    public NegotiationMessage AddInboundMessage(
        string textBody,
        string? subject = null,
        string? rawBody = null,
        Money? proposedTotalRate = null,
        decimal? proposedRatePerMile = null,
        string? rfcMessageId = null,
        string? inReplyToMessageId = null,
        bool quarantined = false)
    {
        var message = new NegotiationMessage
        {
            NegotiationId = Id,
            Sequence = NextSequence(),
            Direction = NegotiationMessageDirection.Inbound,
            Subject = subject,
            TextBody = textBody,
            RawBody = rawBody,
            ProposedTotalRate = proposedTotalRate,
            ProposedRatePerMile = proposedRatePerMile,
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

    private int NextSequence() => ++LastSequence;

    /// <summary>
    /// 128 bits of entropy as 32 lowercase hex characters. Lowercase hex rather than base64url
    /// because the token goes in an email local-part, which is case-insensitive in practice.
    /// </summary>
    private static string GenerateReplyToken() =>
        Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(16));
}
