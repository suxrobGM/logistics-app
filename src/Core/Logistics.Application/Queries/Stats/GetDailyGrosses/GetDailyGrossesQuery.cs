using Logistics.Shared.Models;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetDailyGrossesQuery : IntervalQuery, IRequest<Result<DailyGrossesDto>>
{
    public string? TruckId { get; set; }
    public string? UserId { get; set; }
}
