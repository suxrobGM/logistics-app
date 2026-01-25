using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class ApprovePayrollInvoiceHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService)
    : IAppRequestHandler<ApprovePayrollInvoiceCommand, Result>
{
    public async Task<Result> Handle(ApprovePayrollInvoiceCommand req, CancellationToken ct)
    {
        var payroll = await tenantUow.Repository<PayrollInvoice>().GetByIdAsync(req.Id);

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

        payroll.Status = InvoiceStatus.Approved;
        payroll.ApprovedById = currentUserId.Value;
        payroll.ApprovedAt = DateTime.UtcNow;
        payroll.ApprovalNotes = req.Notes;

        tenantUow.Repository<PayrollInvoice>().Update(payroll);
        await tenantUow.SaveChangesAsync();

        return Result.Ok();
    }
}
