using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class BatchCreatePayrollInvoicesHandler(
    ITenantUnitOfWork tenantUow,
    IPayrollService payrollService)
    : IAppRequestHandler<BatchCreatePayrollInvoicesCommand, Result<BatchCreatePayrollInvoicesResult>>
{
    public async Task<Result<BatchCreatePayrollInvoicesResult>> Handle(
        BatchCreatePayrollInvoicesCommand req, CancellationToken ct)
    {
        var result = new BatchCreatePayrollInvoicesResult();
        var payrollsToCreate = new List<PayrollInvoice>();

        foreach (var employeeId in req.EmployeeIds)
        {
            var employee = await tenantUow.Repository<Employee>().GetByIdAsync(employeeId, ct);

            if (employee is null)
            {
                result.Errors.Add(new BatchCreatePayrollError(
                    employeeId,
                    $"Employee with ID '{employeeId}' not found"));
                continue;
            }

            // Check for overlapping payroll periods
            var overlappingPayroll = await tenantUow.Repository<PayrollInvoice>()
                .GetAsync(p =>
                    p.EmployeeId == employeeId &&
                    p.PeriodStart < req.PeriodEnd &&
                    p.PeriodEnd > req.PeriodStart, ct);

            if (overlappingPayroll is not null)
            {
                result.Errors.Add(new BatchCreatePayrollError(
                    employeeId,
                    $"Employee '{employee.GetFullName()}' already has a payroll invoice for the overlapping period " +
                    $"({overlappingPayroll.PeriodStart:d} - {overlappingPayroll.PeriodEnd:d})"));
                continue;
            }

            var payroll = payrollService.CreatePayrollInvoice(employee, req.PeriodStart, req.PeriodEnd);
            payrollsToCreate.Add(payroll);
        }

        if (payrollsToCreate.Count == 0)
        {
            if (result.Errors.Count > 0)
            {
                return Result<BatchCreatePayrollInvoicesResult>.Fail(
                    "No payroll invoices could be created. " +
                    string.Join("; ", result.Errors.Select(e => e.Message)));
            }

            return Result<BatchCreatePayrollInvoicesResult>.Fail("No employees provided.");
        }

        // Batch add all payrolls
        foreach (var payroll in payrollsToCreate)
        {
            await tenantUow.Repository<PayrollInvoice>().AddAsync(payroll, ct);
            result.CreatedInvoiceIds.Add(payroll.Id);
        }

        await tenantUow.SaveChangesAsync(ct);

        return Result<BatchCreatePayrollInvoicesResult>.Ok(result);
    }
}
