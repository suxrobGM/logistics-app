using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Stats.Queries;

public class GetDailyGrossesQuery : DateRangeQuery, IQuery<Result<DailyGrossesDto>>
{
    public Guid? TruckId { get; set; }
    public Guid? DriverId { get; set; }
}
