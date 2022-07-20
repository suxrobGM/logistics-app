namespace Logistics.Application.Contracts.Queries;

public sealed class GetTrucksQuery : SearchableQueryBase<TruckDto>
{
    public bool IncludeLoadIds { get; set; }
}
