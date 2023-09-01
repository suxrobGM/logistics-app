using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetTruckByIdQuery : Request<ResponseResult<TruckDto>>
{
    public string? Id { get; set; }
    public bool IncludeLoadIds { get; set; } = false;
}
