namespace Logistics.Application.Contracts.Queries;

public sealed class GetTruckGrossesForIntervalQuery : IntervalQuery<TruckGrossesDto>
{
    public string? TruckId { get; set; }
}