using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.TimeEntries.Commands;

internal sealed class UpdateTimeEntryHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<UpdateTimeEntryCommand, Result>
{
    public async Task<Result> Handle(UpdateTimeEntryCommand req, CancellationToken ct)
    {
        var timeEntry = await tenantUow.Repository<TimeEntry>().GetByIdAsync(req.Id, ct);
        if (timeEntry is null)
        {
            return Result.Fail("Time entry not found", ErrorCodes.NotFound);
        }

        // Don't allow editing time entries already linked to a payroll
        if (timeEntry.PayrollInvoiceId.HasValue)
        {
            return Result.Fail("Cannot modify a time entry that has been included in a payroll invoice");
        }

        if (req.StartedAt.HasValue)
            timeEntry.StartedAt = req.StartedAt.Value;

        if (req.EndedAt.HasValue)
            timeEntry.EndedAt = req.EndedAt.Value;

        if (req.Type.HasValue)
            timeEntry.Type = req.Type.Value;

        if (req.Notes is not null)
            timeEntry.Notes = req.Notes;

        // Use provided total hours or recalculate from start/end times
        if (req.TotalHours.HasValue)
        {
            timeEntry.TotalHours = req.TotalHours.Value;
        }
        else if (req.StartedAt.HasValue || req.EndedAt.HasValue)
        {
            timeEntry.CalculateTotalHours();
        }

        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
