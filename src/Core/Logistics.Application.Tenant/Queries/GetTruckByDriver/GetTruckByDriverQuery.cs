using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetTruckByDriverQuery : Request<ResponseResult<TruckDto>>
{
    public required string DriverId { get; set; }
    public bool IncludeLoadIds { get; set; }
}
