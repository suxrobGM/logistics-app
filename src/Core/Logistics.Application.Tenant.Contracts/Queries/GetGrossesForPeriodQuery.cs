namespace Logistics.Application.Contracts.Queries;

public sealed class GetGrossesForPeriodQuery : RequestBase<DataResult<GrossesPerDayDto>>
{
    [Required]
    public DateTime? StartPeriod { get; set; }
    public DateTime EndPeriod { get; set; } = DateTime.UtcNow;
}