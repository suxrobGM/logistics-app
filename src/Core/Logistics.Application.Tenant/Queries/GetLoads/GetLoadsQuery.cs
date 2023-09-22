using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetLoadsQuery : SearchableQuery<LoadDto>
{
    public bool FilterActiveLoads { get; set; }
}
