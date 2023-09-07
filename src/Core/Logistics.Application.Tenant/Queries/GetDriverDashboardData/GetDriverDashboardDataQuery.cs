using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetDriverDashboardDataQuery : Request<ResponseResult<DriverDashboardDto>>
{
    public required string UserId { get; init; }
}
