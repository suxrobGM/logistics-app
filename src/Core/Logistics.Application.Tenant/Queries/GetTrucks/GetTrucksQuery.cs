using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetTrucksQuery : SearchableRequest<TruckDto>
{
    public bool IncludeLoads { get; set; } = false;
}
