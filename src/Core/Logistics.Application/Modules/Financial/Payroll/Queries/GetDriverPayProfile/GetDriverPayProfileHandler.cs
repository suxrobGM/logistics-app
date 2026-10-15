using Logistics.Application.Modules.Financial.Payroll.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Financial.Payroll.Queries;

internal sealed class GetDriverPayProfileHandler(ITenantUnitOfWork tenantUow, IPayrollService payrollService)
    : IRequestHandler<GetDriverPayProfileQuery, Result<DriverPayProfileDto>>
{
    public async Task<Result<DriverPayProfileDto>> Handle(GetDriverPayProfileQuery req, CancellationToken ct)
    {
        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId, ct);
        if (employee is null)
        {
            return Result<DriverPayProfileDto>.Fail($"Could not find an employee with ID '{req.EmployeeId}'");
        }

        var deductions = await tenantUow.Repository<RecurringDeduction>()
            .GetListAsync(d => d.EmployeeId == employee.Id, ct);

        var outstanding = await tenantUow.Repository<CashAdvance>().Query()
            .Where(a => a.EmployeeId == employee.Id &&
                        a.Status == CashAdvanceStatus.Approved &&
                        a.PayrollInvoiceId == null)
            .SumAsync(a => a.Amount.Amount, ct);

        return Result<DriverPayProfileDto>.Ok(new DriverPayProfileDto
        {
            EmployeeId = employee.Id,
            PayExcludesFuelSurcharge = employee.PayExcludesFuelSurcharge,
            PayExcludesAccessorials = employee.PayExcludesAccessorials,
            DeductFuelCardCharges = employee.DeductFuelCardCharges,
            RecurringDeductions = deductions.OrderBy(d => d.Type).ThenBy(d => d.Description).Select(d => d.ToDto()).ToList(),
            EscrowBalance = await payrollService.GetEscrowBalanceAsync(employee.Id, ct: ct),
            OutstandingAdvances = outstanding
        });
    }
}
