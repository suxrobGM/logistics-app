using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetDailyGrossesQuery : IntervalQuery<DailyGrossesDto>
{
    public string? TruckId { get; set; }
    public string? UserId { get; set; }
}
