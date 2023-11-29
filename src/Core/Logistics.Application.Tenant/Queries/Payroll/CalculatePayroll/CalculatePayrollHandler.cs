using Logistics.Application.Tenant.Services;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class CalculatePayrollHandler : RequestHandler<CalculatePayrollQuery, ResponseResult<PayrollDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;
    private readonly IPayrollService _payrollService;

    public CalculatePayrollHandler(
        ITenantUnityOfWork tenantUow,
        IPayrollService payrollService)
    {
        _tenantUow = tenantUow;
        _payrollService = payrollService;
    }

    protected override async Task<ResponseResult<PayrollDto>> HandleValidated(
        CalculatePayrollQuery req, CancellationToken cancellationToken)
    {
        var employee = await _tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId);

        if (employee is null)
        {
            return ResponseResult<PayrollDto>.CreateError($"Could not find an employer with ID '{req.EmployeeId}'");
        }

        var payrollEntity = _payrollService.CreatePayroll(employee, req.StartDate, req.EndDate);
        var payrollDto = payrollEntity.ToDto();
        return ResponseResult<PayrollDto>.CreateSuccess(payrollDto);
    }
}
