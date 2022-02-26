namespace Logistics.Application.Contracts.Queries;

public sealed class GetCargoByIdQuery : RequestBase<DataResult<CargoDto>>
{
    public string? Id { get; set; }
}
