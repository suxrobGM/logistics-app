using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetTrucksQuery : SearchableQuery, IAppRequest<PagedResult<TruckDto>>
{
    public bool IncludeLoads { get; set; } = false;
}
