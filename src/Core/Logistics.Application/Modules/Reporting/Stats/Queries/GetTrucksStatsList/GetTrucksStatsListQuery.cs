using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Stats.Queries;

public class GetTrucksStatsListQuery : PagedIntervalQuery, IQuery<PagedResult<TruckStatsDto>>
{
}
