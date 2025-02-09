using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetTrucksQuery : SearchableQuery, IRequest<PagedResult<TruckDto>>
{
    public bool IncludeLoads { get; set; } = false;
}
