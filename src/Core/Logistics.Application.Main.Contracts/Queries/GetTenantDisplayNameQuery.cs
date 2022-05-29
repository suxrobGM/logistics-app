namespace Logistics.Application.Contracts.Queries;

public class GetTenantDisplayNameQuery : RequestBase<DataResult<string>>
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}
