using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.Negotiation.Queries;

/// <summary>Renders the email that a pending counter-offer decision would send, for the approver to review.</summary>
[RequiresFeature(TenantFeature.AINegotiation)]
public class PreviewCounterOfferQuery : IQuery<Result<CounterOfferPreviewDto>>
{
    public Guid DecisionId { get; set; }
}
