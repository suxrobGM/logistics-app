using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

/// <summary>A negotiation email for clients: the text body only, without the raw provider payload.</summary>
public record NegotiationMessageDto
{
    public Guid Id { get; set; }
    public int Sequence { get; set; }
    public NegotiationMessageDirection Direction { get; set; }
    public string? Subject { get; set; }
    public string TextBody { get; set; } = "";
    public Money? ProposedTotalRate { get; set; }
    public decimal? ProposedRatePerKm { get; set; }
    public Guid? AgentDecisionId { get; set; }
    public bool Quarantined { get; set; }
    public DateTime OccurredAt { get; set; }
}
