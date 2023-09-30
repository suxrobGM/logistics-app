using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetDriverStatsQuery : Request<ResponseResult<DriverStatsDto>>
{
    public string? UserId { get; set; }
}
