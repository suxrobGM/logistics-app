using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.Negotiation.Queries;

[RequiresFeature(TenantFeature.AINegotiation)]
public class GetNegotiationsQuery : PagedQuery, IQuery<PagedResult<RateNegotiationDto>>
{
    public RateNegotiationStatus? Status { get; set; }

    /// <summary>Only threads still waiting on the broker or already answered by them.</summary>
    public bool ActiveOnly { get; set; }
}
