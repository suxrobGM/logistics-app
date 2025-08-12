using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdatePayrollInvoiceHandler : RequestHandler<UpdatePayrollInvoiceCommand, Result>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public UpdatePayrollInvoiceHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        UpdatePayrollInvoiceCommand req, CancellationToken ct)
    {
        var payroll = await _tenantUow.Repository<PayrollInvoice>().GetByIdAsync(req.Id);

        if (payroll is null)
        {
            return Result.Fail($"Could not find a payroll with ID '{req.Id}'");
        }

        if (req.EmployeeId.HasValue && req.EmployeeId != payroll.EmployeeId)
        {
            var employee = await _tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId.Value);

            if (employee is null)
            {
                return Result.Fail($"Could not find an employer with ID '{req.EmployeeId}'");
            }

            payroll.Employee = employee;
        }

        if (req is { PeriodStart: not null, PeriodEnd: not null } &&
            payroll.PeriodStart != req.PeriodStart &&
            payroll.PeriodEnd != req.PeriodEnd)
        {
            payroll.PeriodStart = req.PeriodStart.Value;
            payroll.PeriodEnd = req.PeriodEnd.Value;
        }

        _tenantUow.Repository<PayrollInvoice>().Update(payroll);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
