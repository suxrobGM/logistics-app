using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateTimeEntryHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<CreateTimeEntryCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTimeEntryCommand req, CancellationToken ct)
    {
        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId);
        if (employee is null)
        {
            return Result<Guid>.Fail("Employee not found");
        }

        var timeEntry = new TimeEntry
        {
            EmployeeId = req.EmployeeId,
            Date = req.Date,
            StartTime = req.StartTime,
            EndTime = req.EndTime,
            Type = req.Type,
            Notes = req.Notes
        };

        // Use provided total hours or calculate from start/end times
        if (req.TotalHours.HasValue)
        {
            timeEntry.TotalHours = req.TotalHours.Value;
        }
        else
        {
            timeEntry.CalculateTotalHours();
        }

        await tenantUow.Repository<TimeEntry>().AddAsync(timeEntry, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<Guid>.Ok(timeEntry.Id);
    }
}
