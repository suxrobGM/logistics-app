namespace Logistics.Application.Contracts.Queries;

public sealed class GetTrucksQuery : SearchableQuery<TruckDto>
{
    public bool IncludeLoadIds { get; set; }
}
