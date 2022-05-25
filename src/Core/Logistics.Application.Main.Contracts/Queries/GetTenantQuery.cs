namespace Logistics.Application.Contracts.Queries;

public sealed class GetTenantQuery : RequestBase<DataResult<TenantDto>>
{
    public string? Id { get; set; }
    public string? Name { get; set; }

}
