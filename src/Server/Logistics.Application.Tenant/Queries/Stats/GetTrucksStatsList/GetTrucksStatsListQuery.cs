using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetTrucksStatsListQuery : PagedIntervalQuery, IRequest<PagedResult<TruckStatsDto>>
{
    
}
