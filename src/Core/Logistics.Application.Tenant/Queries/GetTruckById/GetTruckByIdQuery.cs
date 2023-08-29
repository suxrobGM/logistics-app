using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

public sealed class GetTruckByIdQuery : RequestBase<ResponseResult<TruckDto>>
{
    public string? Id { get; set; }
    public bool IncludeLoadIds { get; set; } = false;
}
