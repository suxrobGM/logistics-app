using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class SubmitPayrollForApprovalHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<SubmitPayrollForApprovalCommand, Result>
{
    public async Task<Result> Handle(SubmitPayrollForApprovalCommand req, CancellationToken ct)
    {
        var payroll = await tenantUow.Repository<PayrollInvoice>().GetByIdAsync(req.Id);

        if (payroll is null)
        {
            return Result.Fail($"Could not find payroll invoice with ID '{req.Id}'");
        }

        if (payroll.Status != InvoiceStatus.Draft)
        {
            return Result.Fail("Only draft payroll invoices can be submitted for approval");
        }

        payroll.SubmitForApproval();
        tenantUow.Repository<PayrollInvoice>().Update(payroll);
        await tenantUow.SaveChangesAsync();

        return Result.Ok();
    }
}
