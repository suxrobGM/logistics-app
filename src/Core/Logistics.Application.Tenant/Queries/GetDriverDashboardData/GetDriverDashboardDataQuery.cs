using Logistics.Models;

namespace Logistics.Application.Tenant.Queries.GetDriverDashboardData;

public class GetDriverDashboardDataQuery : Request<ResponseResult<DriverDashboardDto>>
{
    public required string UserId { get; init; }
}