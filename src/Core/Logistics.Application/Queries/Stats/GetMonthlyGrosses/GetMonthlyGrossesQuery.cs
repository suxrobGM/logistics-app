using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetMonthlyGrossesQuery : IntervalQuery, IRequest<Result<MonthlyGrossesDto>>
{
    public Guid? TruckId { get; set; }
    public Guid? UserId { get; set; }
}
