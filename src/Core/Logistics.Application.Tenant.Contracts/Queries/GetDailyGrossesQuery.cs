namespace Logistics.Application.Tenant.Queries;

public sealed class GetDailyGrossesQuery : IntervalQuery<DailyGrossesDto>
{
    public string? TruckId { get; set; }
}