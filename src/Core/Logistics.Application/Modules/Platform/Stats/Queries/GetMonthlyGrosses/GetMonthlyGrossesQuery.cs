using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Stats.Queries;

public class GetMonthlyGrossesQuery : IntervalQuery, IQuery<Result<MonthlyGrossesDto>>
{
    public Guid? TruckId { get; set; }
    public Guid? UserId { get; set; }
}
