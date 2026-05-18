using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Stats.Queries;

public class GetDriverStatsQuery : IQuery<Result<DriverStatsDto>>
{
    public Guid UserId { get; set; }
}
