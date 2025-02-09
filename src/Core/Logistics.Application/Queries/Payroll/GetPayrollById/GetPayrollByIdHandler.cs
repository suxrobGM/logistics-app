using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetPayrollByIdHandler : RequestHandler<GetPayrollByIdQuery, Result<PayrollDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetPayrollByIdHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result<PayrollDto>> HandleValidated(
        GetPayrollByIdQuery req, CancellationToken cancellationToken)
    {
        var payrollEntity = await _tenantUow.Repository<Payroll>().GetByIdAsync(req.Id);
        
        if (payrollEntity is null)
            return Result<PayrollDto>.Fail($"Could not find a payroll with ID {req.Id}");

        var payrollDto = payrollEntity.ToDto();
        return Result<PayrollDto>.Succeed(payrollDto);
    }
}
