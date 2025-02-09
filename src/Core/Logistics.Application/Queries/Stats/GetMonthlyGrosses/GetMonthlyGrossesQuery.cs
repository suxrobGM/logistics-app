using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetMonthlyGrossesQuery : IntervalQuery, IRequest<Result<MonthlyGrossesDto>>
{
    public string? TruckId { get; set; }
    public string? UserId { get; set; }
}
