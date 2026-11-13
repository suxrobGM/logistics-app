using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>An email in a rate negotiation thread. <see cref="TextBody"/> is the text the agent reads; <see cref="RawBody"/> keeps the original payload for audit.</summary>
public class NegotiationMessage : Entity, ITenantEntity
{
    public Guid NegotiationId { get; set; }
    public virtual RateNegotiation Negotiation { get; set; } = null!;

    /// <summary>Unique per negotiation.</summary>
    public int Sequence { get; set; }

    public NegotiationMessageDirection Direction { get; set; }

    public string? Subject { get; set; }

    public required string TextBody { get; set; }

    public string? RawBody { get; set; }

    /// <summary>Rate parsed out of the body, or the rate the agent countered with.</summary>
    public Money? ProposedTotalRate { get; set; }
    public decimal? ProposedRatePerKm { get; set; }

    /// <summary>RFC 5322 Message-ID header value, angle brackets included, used for reply threading; not the email provider's internal id.</summary>
    public string? RfcMessageId { get; set; }
    public string? InReplyToMessageId { get; set; }

    /// <summary>The agent decision that produced an outbound message.</summary>
    public Guid? AgentDecisionId { get; set; }

    /// <summary>Inbound mail that failed sender or content checks; stored but never fed to the agent.</summary>
    public bool Quarantined { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
