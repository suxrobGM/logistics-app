using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Stats.Queries;

public class GetMonthlyGrossesQuery : DateRangeQuery, IQuery<Result<MonthlyGrossesDto>>
{
    public Guid? TruckId { get; set; }
    public Guid? DriverId { get; set; }
}
