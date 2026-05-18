using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Stats.Queries;

public class GetTrucksStatsListQuery : PagedIntervalQuery, IQuery<PagedResult<TruckStatsDto>>
{
}
