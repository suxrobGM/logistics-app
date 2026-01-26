using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdatePayrollInvoiceHandler(
    ITenantUnitOfWork tenantUow,
    IPayrollService payrollService)
    : IAppRequestHandler<UpdatePayrollInvoiceCommand, Result>
{
    public async Task<Result> Handle(
        UpdatePayrollInvoiceCommand req, CancellationToken ct)
    {
        var payroll = await tenantUow.Repository<PayrollInvoice>().GetByIdAsync(req.Id);

        if (payroll is null)
        {
            return Result.Fail($"Could not find a payroll with ID '{req.Id}'");
        }

        // Only allow updates to draft payrolls
        if (payroll.Status != InvoiceStatus.Draft)
        {
            return Result.Fail("Only draft payroll invoices can be updated");
        }

        var needsRecalculation = false;

        if (req.EmployeeId.HasValue && req.EmployeeId != payroll.EmployeeId)
        {
            var employee = await tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId.Value);

            if (employee is null)
            {
                return Result.Fail($"Could not find an employee with ID '{req.EmployeeId}'");
            }

            payroll.EmployeeId = req.EmployeeId.Value;
            payroll.Employee = employee;
            needsRecalculation = true;
        }

        if (req.PeriodStart.HasValue && req.PeriodStart != payroll.PeriodStart)
        {
            payroll.PeriodStart = req.PeriodStart.Value;
            needsRecalculation = true;
        }

        if (req.PeriodEnd.HasValue && req.PeriodEnd != payroll.PeriodEnd)
        {
            payroll.PeriodEnd = req.PeriodEnd.Value;
            needsRecalculation = true;
        }

        // Recalculate payroll amounts if period or employee changed
        if (needsRecalculation)
        {
            payrollService.RecalculatePayroll(payroll);
        }

        tenantUow.Repository<PayrollInvoice>().Update(payroll);
        await tenantUow.SaveChangesAsync();
        return Result.Ok();
    }
}
