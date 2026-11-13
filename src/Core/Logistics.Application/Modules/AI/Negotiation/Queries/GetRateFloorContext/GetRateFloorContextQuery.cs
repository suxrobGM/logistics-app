using Logistics.Application.Common.Pipeline;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.Negotiation.Queries;

[RequiresFeature(TenantFeature.AINegotiation)]
public class GetRateFloorContextQuery : IQuery<Result<RateFloorContextDto>>
{
    public Guid ListingId { get; set; }
}
