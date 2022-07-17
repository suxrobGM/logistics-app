namespace Logistics.Application.Contracts.Queries;

public class GetTenantDisplayNameQuery : RequestBase<DataResult<TenantDto>>
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}
