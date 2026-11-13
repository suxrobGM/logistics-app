using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Reporting.Stats.Queries;

public class GetDriverStatsQuery : IQuery<Result<DriverStatsDto>>
{
    public Guid UserId { get; set; }
}
