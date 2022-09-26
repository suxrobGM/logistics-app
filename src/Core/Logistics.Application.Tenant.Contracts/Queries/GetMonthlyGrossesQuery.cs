namespace Logistics.Application.Contracts.Queries;

public sealed class GetMonthlyGrossesQuery : IntervalQuery<MonthlyGrossesDto>
{
    public string? TruckId { get; set; }
}