using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetMonthlyGrossesQuery : IntervalQuery, IRequest<ResponseResult<MonthlyGrossesDto>>
{
    public string? TruckId { get; set; }
    public string? UserId { get; set; }
}
