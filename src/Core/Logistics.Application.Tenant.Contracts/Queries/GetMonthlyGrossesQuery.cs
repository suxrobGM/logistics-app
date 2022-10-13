namespace Logistics.Application.Tenant.Queries;

public sealed class GetMonthlyGrossesQuery : IntervalQuery<MonthlyGrossesDto>
{
    public string? TruckId { get; set; }
}