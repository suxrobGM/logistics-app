using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetDailyGrossesQuery : IntervalQuery, IRequest<Result<DailyGrossesDto>>
{
    public Guid? TruckId { get; set; }
    public Guid? UserId { get; set; }
}
