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

    private const string Base32Alphabet = "abcdefghijklmnopqrstuvwxyz234567";
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

    public static RateNegotiation Create(
        Guid loadBoardListingId,
        string brokerEmail,
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
            ReplyToken = GenerateReplyToken()
        };
    }

    /// <summary>
    /// Records a counter sent to the broker: consumes a round and restarts the reply window.
    /// Callers MUST also register the returned row via repository AddAsync (pre-generated ids make
    /// a collection-only add save as an UPDATE and fail).
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

        Messages.Add(message);
        RoundCount++;
        LatestCounterOffer = proposedTotalRate ?? LatestCounterOffer;
        Status = RateNegotiationStatus.AwaitingBroker;
        ExpiresAt = DateTime.UtcNow.Add(ReplyWindow);
        return message;
    }

    /// <summary>
    /// Records a broker reply. A quarantined message is stored for audit but moves nothing.
    /// Callers MUST also register the returned row via repository AddAsync (pre-generated ids make
    /// a collection-only add save as an UPDATE and fail).
    /// </summary>
    public NegotiationMessage AddInboundMessage(
        string textBody,
        string? subject = null,
        string? rawBody = null,
        Money? proposedTotalRate = null,
        decimal? proposedRatePerMile = null,
        string? providerMessageId = null,
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
            ProviderMessageId = providerMessageId,
            InReplyToMessageId = inReplyToMessageId,
            Quarantined = quarantined
        };

        Messages.Add(message);

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

    public int NextSequence() => Messages.Count > 0 ? Messages.Max(m => m.Sequence) + 1 : 1;

    /// <summary>160 bits of entropy as unpadded lowercase base32 - 32 address-safe characters.</summary>
    private static string GenerateReplyToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(20);
        var token = new char[32];
        var buffer = 0;
        var bits = 0;
        var index = 0;

        foreach (var b in bytes)
        {
            buffer = (buffer << 8) | b;
            bits += 8;

            while (bits >= 5)
            {
                bits -= 5;
                token[index++] = Base32Alphabet[(buffer >> bits) & 31];
            }
        }

        return new string(token);
    }
}
