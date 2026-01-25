using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreatePayrollInvoiceHandler : IAppRequestHandler<CreatePayrollInvoiceCommand, Result>
{
    private readonly IPayrollService _payrollService;
    private readonly ITenantUnitOfWork _tenantUow;

    public CreatePayrollInvoiceHandler(
        ITenantUnitOfWork tenantUow,
        IPayrollService payrollService)
    {
        _tenantUow = tenantUow;
        _payrollService = payrollService;
    }

    public async Task<Result> Handle(
        CreatePayrollInvoiceCommand req, CancellationToken ct)
    {
        var employee = await _tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId);

        if (employee is null)
        {
            return Result.Fail($"Could not find an employee with ID '{req.EmployeeId}'");
        }

        // Check for overlapping payroll periods
        var overlappingPayroll = await _tenantUow.Repository<PayrollInvoice>()
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

        var payroll = _payrollService.CreatePayrollInvoice(employee, req.PeriodStart, req.PeriodEnd);
        await _tenantUow.Repository<PayrollInvoice>().AddAsync(payroll);
        await _tenantUow.SaveChangesAsync();
        return Result.Ok();
    }
}
