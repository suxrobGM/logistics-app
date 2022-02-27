namespace Logistics.Application.Contracts.Queries;

public sealed class GetTrucksQuery : PagedQueryBase<TruckDto>
{
    public FilterTruck? Filter { get; set; }
}
