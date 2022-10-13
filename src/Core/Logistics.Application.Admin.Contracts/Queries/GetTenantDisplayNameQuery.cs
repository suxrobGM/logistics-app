namespace Logistics.Application.Admin.Queries;

public class GetTenantDisplayNameQuery : RequestBase<ResponseResult<TenantDto>>
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}
