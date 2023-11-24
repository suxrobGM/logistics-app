using Logistics.Application.Tenant.Services;

namespace Logistics.Application.Tenant.Commands;

internal sealed class CreatePayrollHandler : RequestHandler<CreatePayrollCommand, ResponseResult>
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

    protected override async Task<ResponseResult> HandleValidated(
        CreatePayrollCommand req, CancellationToken cancellationToken)
    {
        var employee = await _tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId);

        if (employee is null)
        {
            return ResponseResult.CreateError($"Could not find an employer with ID '{req.EmployeeId}'");
        }

        var payroll = _payrollService.CreatePayroll(employee, req.StartDate, req.EndDate);
        await _tenantUow.Repository<Payroll>().AddAsync(payroll);
        await _tenantUow.SaveChangesAsync();
        return ResponseResult.CreateSuccess();
    }
}
