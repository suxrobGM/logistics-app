using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetLoadByIdQuery : Request<ResponseResult<LoadDto>>
{
    public string? Id { get; set; }
}
