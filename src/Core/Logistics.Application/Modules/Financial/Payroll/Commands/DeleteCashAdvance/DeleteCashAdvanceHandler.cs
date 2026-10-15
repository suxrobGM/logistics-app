using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

internal sealed class DeleteCashAdvanceHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<DeleteCashAdvanceCommand, Result>
{
    public async Task<Result> Handle(DeleteCashAdvanceCommand req, CancellationToken ct)
    {
        var advance = await tenantUow.Repository<CashAdvance>().GetByIdAsync(req.Id, ct);
        if (advance is null)
        {
            return Result.Fail($"Could not find a cash advance with ID '{req.Id}'");
        }

        if (advance.PayrollInvoiceId is not null)
        {
            return Result.Fail("This advance is already deducted on a settlement; remove it there first.");
        }

        tenantUow.Repository<CashAdvance>().Delete(advance);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
