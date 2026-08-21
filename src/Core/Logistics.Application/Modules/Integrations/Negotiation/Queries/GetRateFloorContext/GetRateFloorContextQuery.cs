using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Negotiation.Queries;

[RequiresFeature(TenantFeature.AIRateNegotiation)]
public class GetRateFloorContextQuery : IQuery<Result<RateFloorContextDto>>
{
    public Guid ListingId { get; set; }
}
