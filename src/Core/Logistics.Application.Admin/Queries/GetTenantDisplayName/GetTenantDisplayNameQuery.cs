using Logistics.Models;

namespace Logistics.Application.Admin.Queries;

public class GetTenantDisplayNameQuery : Request<ResponseResult<TenantDto>>
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}
