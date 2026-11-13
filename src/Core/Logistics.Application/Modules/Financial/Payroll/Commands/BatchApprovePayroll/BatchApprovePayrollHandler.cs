using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Context;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

internal sealed class BatchApprovePayrollHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService)
    : IRequestHandler<BatchApprovePayrollCommand, Result>
{
    public async Task<Result> Handle(BatchApprovePayrollCommand req, CancellationToken ct)
    {
        var currentUserId = currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Result.Fail("Unable to determine the current user");
        }

        var payrolls = await tenantUow.Repository<PayrollInvoice>()
            .GetListAsync(p => req.Ids.Contains(p.Id), ct);

        if (payrolls.Count == 0)
        {
            return Result.Fail("No payroll invoices found with the provided IDs");
        }

        foreach (var payroll in payrolls)
        {
            if (payroll.Status != InvoiceStatus.PendingApproval)
            {
                continue;
            }

            payroll.Approve(currentUserId.Value, req.Notes);
        }

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
