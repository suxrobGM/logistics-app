using Logistics.Application.Modules.Financial.Payroll.Services;
using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Commands;

internal sealed class CreatePayrollInvoiceHandler(
    ITenantUnitOfWork tenantUow,
    IPayrollService payrollService)
    : IAppRequestHandler<CreatePayrollInvoiceCommand, Result>
{
    public async Task<Result> Handle(
        CreatePayrollInvoiceCommand req, CancellationToken ct)
    {
        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId);

        if (employee is null)
        {
            return Result.Fail($"Could not find an employee with ID '{req.EmployeeId}'");
        }

        // Check for overlapping payroll periods
        var overlappingPayroll = await tenantUow.Repository<PayrollInvoice>()
            .GetAsync(p =>
                p.EmployeeId == req.EmployeeId &&
                p.PeriodStart < req.PeriodEnd &&
                p.PeriodEnd > req.PeriodStart);

        if (overlappingPayroll is not null)
        {
            return Result.Fail(
                $"A payroll invoice already exists for this employee that overlaps with the specified period " +
                $"({overlappingPayroll.PeriodStart:d} - {overlappingPayroll.PeriodEnd:d})");
        }

        var payroll = payrollService.CreatePayrollInvoice(employee, req.PeriodStart, req.PeriodEnd);
        await tenantUow.Repository<PayrollInvoice>().AddAsync(payroll);
        await tenantUow.SaveChangesAsync();

        // Link time entries to the payroll (for hourly employees)
        await payrollService.LinkTimeEntriesToPayrollAsync(payroll);
        await tenantUow.SaveChangesAsync();

        return Result.Ok();
    }
}
