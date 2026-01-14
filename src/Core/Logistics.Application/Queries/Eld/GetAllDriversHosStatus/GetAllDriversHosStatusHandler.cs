using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetAllDriversHosStatusHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetAllDriversHosStatusQuery, Result<List<DriverHosStatusDto>>>
{
    public async Task<Result<List<DriverHosStatusDto>>> Handle(
        GetAllDriversHosStatusQuery req,
        CancellationToken ct)
    {
        var hosStatuses = await tenantUow.Repository<DriverHosStatus>()
            .ApplySpecification(new AllDriverHosStatusesSpec())
            .Select(h => new DriverHosStatusDto
            {
                EmployeeId = h.EmployeeId,
                EmployeeName = h.Employee.FirstName + " " + h.Employee.LastName,
                CurrentDutyStatus = h.CurrentDutyStatus,
                CurrentDutyStatusDisplay = h.CurrentDutyStatus.ToString(),
                StatusChangedAt = h.StatusChangedAt,
                DrivingMinutesRemaining = h.DrivingMinutesRemaining,
                OnDutyMinutesRemaining = h.OnDutyMinutesRemaining,
                CycleMinutesRemaining = h.CycleMinutesRemaining,
                DrivingTimeRemainingDisplay = (h.DrivingMinutesRemaining / 60) + "h " + (h.DrivingMinutesRemaining % 60) + "m",
                OnDutyTimeRemainingDisplay = (h.OnDutyMinutesRemaining / 60) + "h " + (h.OnDutyMinutesRemaining % 60) + "m",
                CycleTimeRemainingDisplay = (h.CycleMinutesRemaining / 60) + "h " + (h.CycleMinutesRemaining % 60) + "m",
                TimeUntilBreakRequired = h.TimeUntilBreakRequired,
                IsInViolation = h.IsInViolation,
                IsAvailableForDispatch = !h.IsInViolation && h.DrivingMinutesRemaining >= 60,
                LastUpdatedAt = h.LastUpdatedAt,
                NextMandatoryBreakAt = h.NextMandatoryBreakAt,
                ProviderType = h.ProviderType
            })
            .ToListAsync(ct);

        return Result<List<DriverHosStatusDto>>.Ok(hosStatuses);
    }
}

// Simple specification to include Employee navigation
internal class AllDriverHosStatusesSpec : Logistics.Domain.Specifications.BaseSpecification<DriverHosStatus>
{
    public AllDriverHosStatusesSpec()
    {
        AddInclude(h => h.Employee);
    }
}
