using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetTruckByDriverQuery : RequestBase<ResponseResult<TruckDto>>
{
    public string? DriverId { get; set; }
    public bool IncludeLoadIds { get; set; } = false;
}
