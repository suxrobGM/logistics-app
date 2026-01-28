using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetTrucksQuery : SearchableQuery, IAppRequest<PagedResult<TruckDto>>
{
    public bool IncludeLoads { get; set; } = false;
    public TruckStatus[]? Statuses { get; set; }
    public TruckType[]? Types { get; set; }
}
