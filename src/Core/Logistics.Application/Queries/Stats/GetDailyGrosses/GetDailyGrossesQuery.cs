using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetDailyGrossesQuery : IntervalQuery, IAppRequest<Result<DailyGrossesDto>>
{
    public Guid? TruckId { get; set; }
    public Guid? UserId { get; set; }
}
