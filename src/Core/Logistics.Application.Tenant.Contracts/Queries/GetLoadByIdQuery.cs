namespace Logistics.Application.Tenant.Queries;

public sealed class GetLoadByIdQuery : RequestBase<ResponseResult<LoadDto>>
{
    public string? Id { get; set; }
}
