using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetTrucksQuery : SearchableQuery, IRequest<PagedResponseResult<TruckDto>>
{
    public bool IncludeLoads { get; set; } = false;
}
