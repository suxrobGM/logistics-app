using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.Negotiation.Queries;

/// <summary>
/// Renders the email a pending counter-offer decision would send, so approving it is a decision
/// about the real message rather than about a summary of it.
/// </summary>
[RequiresFeature(TenantFeature.AINegotiation)]
public class PreviewCounterOfferQuery : IQuery<Result<CounterOfferPreviewDto>>
{
    public Guid DecisionId { get; set; }
}
