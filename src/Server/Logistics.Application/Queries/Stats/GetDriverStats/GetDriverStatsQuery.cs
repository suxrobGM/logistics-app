using Logistics.Shared;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class GetDriverStatsQuery : IRequest<Result<DriverStatsDto>>
{
    public string? UserId { get; set; }
}
