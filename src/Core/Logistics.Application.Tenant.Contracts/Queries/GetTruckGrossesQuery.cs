namespace Logistics.Application.Contracts.Queries;

public sealed class GetTruckGrossesQuery : IntervalQuery<TruckGrossesDto>
{
    public string? TruckId { get; set; }
}