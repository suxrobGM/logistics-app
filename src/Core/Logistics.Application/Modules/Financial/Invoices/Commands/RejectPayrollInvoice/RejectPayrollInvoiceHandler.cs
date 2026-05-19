using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.CurrentUser;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

internal sealed class RejectPayrollInvoiceHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService)
    : IAppRequestHandler<RejectPayrollInvoiceCommand, Result>
{
    public async Task<Result> Handle(RejectPayrollInvoiceCommand req, CancellationToken ct)
    {
        var payroll = await tenantUow.Repository<PayrollInvoice>().GetByIdAsync(req.Id);

        if (payroll is null)
        {
            return Result.Fail($"Could not find payroll invoice with ID '{req.Id}'");
        }

        if (payroll.Status != InvoiceStatus.PendingApproval)
        {
            return Result.Fail("Only payroll invoices pending approval can be rejected");
        }

        var currentUserId = currentUserService.GetUserId() ?? Guid.Empty;
        payroll.Reject(currentUserId, req.Reason);
        tenantUow.Repository<PayrollInvoice>().Update(payroll);
        await tenantUow.SaveChangesAsync();

        return Result.Ok();
    }
}
