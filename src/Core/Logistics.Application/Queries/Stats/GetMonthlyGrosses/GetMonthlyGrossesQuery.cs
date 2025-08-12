using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetMonthlyGrossesQuery : IntervalQuery, IAppRequest<Result<MonthlyGrossesDto>>
{
    public Guid? TruckId { get; set; }
    public Guid? UserId { get; set; }
}
