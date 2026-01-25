using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class BatchApprovePayrollHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService)
    : IAppRequestHandler<BatchApprovePayrollCommand, Result>
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

            payroll.Status = InvoiceStatus.Approved;
            payroll.ApprovedById = currentUserId.Value;
            payroll.ApprovedAt = DateTime.UtcNow;
            payroll.ApprovalNotes = req.Notes;

            tenantUow.Repository<PayrollInvoice>().Update(payroll);
        }

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
