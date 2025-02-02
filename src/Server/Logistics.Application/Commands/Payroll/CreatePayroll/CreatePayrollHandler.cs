using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared;

namespace Logistics.Application.Commands;

internal sealed class CreatePayrollHandler : RequestHandler<CreatePayrollCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly IPayrollService _payrollService;

    public CreatePayrollHandler(
        ITenantUnityOfWork tenantUow,
        IPayrollService payrollService)
    {
        _tenantUow = tenantUow;
        _payrollService = payrollService;
    }

    protected override async Task<Result> HandleValidated(
        CreatePayrollCommand req, CancellationToken cancellationToken)
    {
        var employee = await _tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId);

        if (employee is null)
        {
            return Result.Fail($"Could not find an employer with ID '{req.EmployeeId}'");
        }

        var payroll = _payrollService.CreatePayroll(employee, req.StartDate, req.EndDate);
        await _tenantUow.Repository<Payroll>().AddAsync(payroll);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
