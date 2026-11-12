using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.CurrentUser;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

internal sealed class ApprovePayrollInvoiceHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService)
    : IRequestHandler<ApprovePayrollInvoiceCommand, Result>
{
    public async Task<Result> Handle(ApprovePayrollInvoiceCommand req, CancellationToken ct)
    {
        var payroll = await tenantUow.Repository<PayrollInvoice>().GetByIdAsync(req.Id, ct);

        if (payroll is null)
        {
            return Result.Fail($"Could not find payroll invoice with ID '{req.Id}'");
        }

        if (payroll.Status != InvoiceStatus.PendingApproval)
        {
            return Result.Fail("Only payroll invoices pending approval can be approved");
        }

        var currentUserId = currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Result.Fail("Unable to determine the current user");
        }

        payroll.Approve(currentUserId.Value, req.Notes);
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
