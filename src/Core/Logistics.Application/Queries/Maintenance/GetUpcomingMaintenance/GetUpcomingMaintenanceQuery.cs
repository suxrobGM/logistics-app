using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public record GetUpcomingMaintenanceQuery : IAppRequest<Result<List<MaintenanceScheduleDto>>>
{
    public int DaysAhead { get; set; } = 30;
    public Guid? TruckId { get; set; }
    public bool IncludeOverdue { get; set; } = true;
}
