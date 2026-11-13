using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Context;
using Logistics.Application.Modules.Financial.Payroll.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

internal sealed class RejectPayrollInvoiceHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService,
    IPayrollService payrollService)
    : IRequestHandler<RejectPayrollInvoiceCommand, Result>
{
    public async Task<Result> Handle(RejectPayrollInvoiceCommand req, CancellationToken ct)
    {
        var payroll = await tenantUow.Repository<PayrollInvoice>().GetByIdAsync(req.Id, ct);

        if (payroll is null)
        {
            return Result.Fail($"Could not find payroll invoice with ID '{req.Id}'", ErrorCodes.NotFound);
        }

        if (payroll.Status != InvoiceStatus.PendingApproval)
        {
            return Result.Fail("Only payroll invoices pending approval can be rejected");
        }

        var currentUserId = currentUserService.GetUserId() ?? Guid.Empty;
        payroll.Reject(currentUserId, req.Reason);
        await payrollService.ReleaseDeductionsAsync(payroll, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
