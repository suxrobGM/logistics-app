using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trucks.Queries;

public class GetTrucksQuery : SearchableQuery, IQuery<PagedResult<TruckDto>>
{
    public bool IncludeLoads { get; set; } = false;
    public TruckStatus[]? Statuses { get; set; }
    public TruckType[]? Types { get; set; }
}
