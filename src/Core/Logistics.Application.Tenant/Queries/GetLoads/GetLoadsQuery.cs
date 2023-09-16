using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetLoadsQuery : SearchableRequest<LoadDto>
{
    public bool FilterActiveLoads { get; set; }
}
