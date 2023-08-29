using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

public sealed class GetTrucksRequest : SearchableRequest<TruckDto>
{
    public bool IncludeLoadIds { get; set; } = false;
}
