namespace Logistics.Application.Contracts.Queries;

public sealed class GetGrossesForPeriodQuery : RequestBase<DataResult<GrossesPerDayDto>>
{
    public GetGrossesForPeriodQuery()
    {
        StartPeriod = DateTime.UtcNow;
        EndPeriod = DateTime.UtcNow;
    }
    
    public DateTime StartPeriod { get; set; }
    public DateTime EndPeriod { get; set; }
}