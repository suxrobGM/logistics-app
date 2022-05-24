namespace Logistics.Application.Contracts.Queries;

public sealed class GetTenatByIdQuery : RequestBase<DataResult<TenantDto>>
{
    public string? Id { get; set; }
}
