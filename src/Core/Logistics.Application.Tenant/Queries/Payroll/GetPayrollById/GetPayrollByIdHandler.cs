using Logistics.Application.Tenant.Mappers;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetPayrollByIdHandler : RequestHandler<GetPayrollByIdQuery, ResponseResult<PayrollDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetPayrollByIdHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult<PayrollDto>> HandleValidated(
        GetPayrollByIdQuery req, CancellationToken cancellationToken)
    {
        var payrollEntity = await _tenantRepository.GetAsync<Payroll>(req.Id);
        
        if (payrollEntity is null)
            return ResponseResult<PayrollDto>.CreateError($"Could not find a payroll with ID {req.Id}");

        var payrollDto = payrollEntity.ToDto();
        return ResponseResult<PayrollDto>.CreateSuccess(payrollDto);
    }
}
