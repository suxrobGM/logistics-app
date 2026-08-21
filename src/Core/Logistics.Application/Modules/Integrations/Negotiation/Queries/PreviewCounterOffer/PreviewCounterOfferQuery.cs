using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Negotiation.Queries;

/// <summary>
/// Renders the email a pending counter-offer decision would send, so approving it is a decision
/// about the real message rather than about a summary of it.
/// </summary>
[RequiresFeature(TenantFeature.AIRateNegotiation)]
public class PreviewCounterOfferQuery : IQuery<Result<CounterOfferPreviewDto>>
{
    public Guid DecisionId { get; set; }
}
