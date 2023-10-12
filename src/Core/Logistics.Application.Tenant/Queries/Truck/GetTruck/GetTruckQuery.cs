using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetTruckQuery : Request<ResponseResult<TruckDto>>
{
    public string? TruckOrDriverId { get; set; }
    public bool IncludeLoads { get; set; }
    public bool OnlyActiveLoads { get; set; }
}
