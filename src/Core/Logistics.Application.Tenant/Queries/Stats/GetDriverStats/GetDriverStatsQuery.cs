using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetDriverStatsQuery : IRequest<Result<DriverStatsDto>>
{
    public string? UserId { get; set; }
}
