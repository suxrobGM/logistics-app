using Logistics.Application.Tenant.Services;

namespace Logistics.Application.Tenant.Commands;

internal sealed class CreatePayrollHandler : RequestHandler<CreatePayrollCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IPayrollService _payrollService;

    public CreatePayrollHandler(
        ITenantRepository tenantRepository,
        IPayrollService payrollService)
    {
        _tenantRepository = tenantRepository;
        _payrollService = payrollService;
    }

    protected override async Task<ResponseResult> HandleValidated(
        CreatePayrollCommand req, CancellationToken cancellationToken)
    {
        var employee = await _tenantRepository.GetAsync<Employee>(req.EmployeeId);

        if (employee is null)
        {
            return ResponseResult.CreateError($"Could not find an employer with ID '{req.EmployeeId}'");
        }

        var payroll = _payrollService.CreatePayroll(employee, req.StartDate, req.EndDate);
        await _tenantRepository.AddAsync(payroll);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
