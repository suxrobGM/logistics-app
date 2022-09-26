namespace Logistics.Application.Contracts.Queries;

public sealed class GetDailyGrossesQuery : IntervalQuery<DailyGrossesDto>
{
    public string? TruckId { get; set; }
}