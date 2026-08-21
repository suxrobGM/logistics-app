using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Negotiation.Queries;

[RequiresFeature(TenantFeature.AIRateNegotiation)]
public class GetNegotiationsQuery : PagedQuery, IQuery<PagedResult<RateNegotiationDto>>
{
    public RateNegotiationStatus? Status { get; set; }

    /// <summary>Only threads still waiting on the broker or already answered by them.</summary>
    public bool ActiveOnly { get; set; }
}
