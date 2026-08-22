using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// One email in a <see cref="RateNegotiation"/> thread. <see cref="TextBody"/> is what the agent
/// reads and the UI renders; <see cref="RawBody"/> keeps the provider payload for audit.
/// </summary>
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
    public decimal? ProposedRatePerMile { get; set; }

    /// <summary>
    /// RFC 5322 Message-ID of this email, angle brackets included: taken from the provider payload
    /// inbound, generated before the send outbound. Quoted as In-Reply-To/References by later
    /// rounds, so it must be the header value a mail server saw, never the provider's own id.
    /// </summary>
    public string? RfcMessageId { get; set; }
    public string? InReplyToMessageId { get; set; }

    /// <summary>The agent decision that produced an outbound message.</summary>
    public Guid? AgentDecisionId { get; set; }

    /// <summary>Inbound mail that failed sender or content checks; stored but never fed to the agent.</summary>
    public bool Quarantined { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
