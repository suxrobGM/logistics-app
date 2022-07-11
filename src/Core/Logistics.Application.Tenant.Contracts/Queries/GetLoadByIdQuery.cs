namespace Logistics.Application.Contracts.Queries;

public sealed class GetLoadByIdQuery : RequestBase<DataResult<LoadDto>>
{
    public string? Id { get; set; }
}
