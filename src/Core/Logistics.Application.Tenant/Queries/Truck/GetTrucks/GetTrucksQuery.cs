using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetTrucksQuery : SearchableQuery<TruckDto>
{
    public bool IncludeLoads { get; set; } = false;
}
