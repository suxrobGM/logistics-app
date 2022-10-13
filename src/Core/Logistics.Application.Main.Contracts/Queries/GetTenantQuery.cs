namespace Logistics.Application.Main.Queries;

public sealed class GetTenantQuery : RequestBase<ResponseResult<TenantDto>>
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}
