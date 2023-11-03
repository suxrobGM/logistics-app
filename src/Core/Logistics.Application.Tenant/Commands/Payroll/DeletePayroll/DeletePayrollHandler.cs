namespace Logistics.Application.Tenant.Commands;

internal sealed class DeletePayrollHandler : RequestHandler<DeletePaymentCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public DeletePayrollHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        DeletePaymentCommand req, CancellationToken cancellationToken)
    {
        var payroll = await _tenantRepository.GetAsync<Payroll>(req.Id);

        if (payroll is null)
        {
            return ResponseResult.CreateError($"Could not find a payroll with ID {req.Id}");
        }
        
        _tenantRepository.Delete(payroll);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
