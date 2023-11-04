using Logistics.Application.Tenant.Mappers;
using Logistics.Application.Tenant.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class CalculatePayrollHandler : RequestHandler<CalculatePayrollQuery, ResponseResult<PayrollDto>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IPayrollService _payrollService;

    public CalculatePayrollHandler(
        ITenantRepository tenantRepository,
        IPayrollService payrollService)
    {
        _tenantRepository = tenantRepository;
        _payrollService = payrollService;
    }

    protected override async Task<ResponseResult<PayrollDto>> HandleValidated(
        CalculatePayrollQuery req, CancellationToken cancellationToken)
    {
        var employee = await _tenantRepository.GetAsync<Employee>(req.EmployeeId);

        if (employee is null)
        {
            return ResponseResult<PayrollDto>.CreateError($"Could not find an employer with ID '{req.EmployeeId}'");
        }

        var payrollEntity = _payrollService.CreatePayroll(employee, req.StartDate, req.EndDate);
        var payrollDto = payrollEntity.ToDto();
        return ResponseResult<PayrollDto>.CreateSuccess(payrollDto);
    }
}
