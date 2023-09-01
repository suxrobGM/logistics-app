using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetDailyGrossesQuery : IntervalQuery<DailyGrossesDto>
{
    public string? TruckId { get; set; }
}