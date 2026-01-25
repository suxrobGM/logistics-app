using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteTimeEntryHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<DeleteTimeEntryCommand, Result>
{
    public async Task<Result> Handle(DeleteTimeEntryCommand req, CancellationToken ct)
    {
        var timeEntry = await tenantUow.Repository<TimeEntry>().GetByIdAsync(req.Id);
        if (timeEntry is null)
        {
            return Result.Fail("Time entry not found");
        }

        // Don't allow deleting time entries already linked to a payroll
        if (timeEntry.PayrollInvoiceId.HasValue)
        {
            return Result.Fail("Cannot delete a time entry that has been included in a payroll invoice");
        }

        tenantUow.Repository<TimeEntry>().Delete(timeEntry);
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
