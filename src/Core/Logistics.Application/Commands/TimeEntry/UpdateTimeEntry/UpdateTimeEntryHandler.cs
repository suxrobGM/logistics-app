using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateTimeEntryHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<UpdateTimeEntryCommand, Result>
{
    public async Task<Result> Handle(UpdateTimeEntryCommand req, CancellationToken ct)
    {
        var timeEntry = await tenantUow.Repository<TimeEntry>().GetByIdAsync(req.Id);
        if (timeEntry is null)
        {
            return Result.Fail("Time entry not found");
        }

        // Don't allow editing time entries already linked to a payroll
        if (timeEntry.PayrollInvoiceId.HasValue)
        {
            return Result.Fail("Cannot modify a time entry that has been included in a payroll invoice");
        }

        if (req.Date.HasValue)
            timeEntry.Date = req.Date.Value;

        if (req.StartTime.HasValue)
            timeEntry.StartTime = req.StartTime.Value;

        if (req.EndTime.HasValue)
            timeEntry.EndTime = req.EndTime.Value;

        if (req.Type.HasValue)
            timeEntry.Type = req.Type.Value;

        if (req.Notes is not null)
            timeEntry.Notes = req.Notes;

        // Use provided total hours or recalculate from start/end times
        if (req.TotalHours.HasValue)
        {
            timeEntry.TotalHours = req.TotalHours.Value;
        }
        else if (req.StartTime.HasValue || req.EndTime.HasValue)
        {
            timeEntry.CalculateTotalHours();
        }

        tenantUow.Repository<TimeEntry>().Update(timeEntry);
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
