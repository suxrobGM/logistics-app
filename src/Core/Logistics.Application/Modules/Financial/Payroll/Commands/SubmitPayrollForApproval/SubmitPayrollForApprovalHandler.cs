using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

internal sealed class SubmitPayrollForApprovalHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<SubmitPayrollForApprovalCommand, Result>
{
    public async Task<Result> Handle(SubmitPayrollForApprovalCommand req, CancellationToken ct)
    {
        var payroll = await tenantUow.Repository<PayrollInvoice>().GetByIdAsync(req.Id, ct);

        if (payroll is null)
        {
            return Result.Fail($"Could not find payroll invoice with ID '{req.Id}'", ErrorCodes.NotFound);
        }

        if (payroll.Status != InvoiceStatus.Draft)
        {
            return Result.Fail("Only draft payroll invoices can be submitted for approval");
        }

        payroll.SubmitForApproval();
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
