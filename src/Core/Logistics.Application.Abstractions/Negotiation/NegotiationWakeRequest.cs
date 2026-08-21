namespace Logistics.Application.Abstractions.Negotiation;

/// <summary>
/// A retry of "wake the dispatch agent for this negotiation", used when the conversation was busy
/// with another turn at the moment the broker replied.
/// </summary>
public record NegotiationWakeRequest(Guid TenantId, Guid NegotiationId);
