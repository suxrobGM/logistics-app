using Logistics.Models;

namespace Logistics.Application.Admin.Queries;

public sealed class GetTenantQuery : Request<ResponseResult<TenantDto>>
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public bool IncludeConnectionString { get; set; }
}
