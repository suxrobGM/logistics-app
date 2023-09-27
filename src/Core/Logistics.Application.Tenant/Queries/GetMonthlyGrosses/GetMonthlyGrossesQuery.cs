using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetMonthlyGrossesQuery : IntervalQuery<MonthlyGrossesDto>
{
    public string? TruckId { get; set; }
    public string? UserId { get; set; }
}
