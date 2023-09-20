using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetDriverActiveLoadsQuery : Request<ResponseResult<DriverActiveLoadsDto>>
{
    public string? UserId { get; init; }
}
