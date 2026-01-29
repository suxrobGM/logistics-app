using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Maintenance;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetUpcomingMaintenanceHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetUpcomingMaintenanceQuery, Result<List<MaintenanceScheduleDto>>>
{
    public Task<Result<List<MaintenanceScheduleDto>>> Handle(GetUpcomingMaintenanceQuery req, CancellationToken ct)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(req.DaysAhead);

        var baseQuery = tenantUow.Repository<MaintenanceSchedule>()
            .Query()
            .Where(s => s.IsActive);

        if (req.TruckId.HasValue)
        {
            baseQuery = baseQuery.Where(s => s.TruckId == req.TruckId);
        }

        if (req.IncludeOverdue)
        {
            baseQuery = baseQuery.Where(s =>
                s.NextDueDate <= cutoffDate ||
                (s.NextDueDate.HasValue && s.NextDueDate < DateTime.UtcNow));
        }
        else
        {
            baseQuery = baseQuery.Where(s =>
                s.NextDueDate.HasValue &&
                s.NextDueDate >= DateTime.UtcNow &&
                s.NextDueDate <= cutoffDate);
        }

        var schedules = baseQuery
            .OrderBy(s => s.NextDueDate)
            .Select(s => s.ToDto(null))
            .ToList();

        return Task.FromResult(Result<List<MaintenanceScheduleDto>>.Ok(schedules));
    }
}
