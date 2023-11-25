namespace Logistics.Application.Tenant.Commands;

internal sealed class DeletePayrollHandler : RequestHandler<DeletePaymentCommand, ResponseResult>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public DeletePayrollHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<ResponseResult> HandleValidated(
        DeletePaymentCommand req, CancellationToken cancellationToken)
    {
        var payroll = await _tenantUow.Repository<Payroll>().GetByIdAsync(req.Id);

        if (payroll is null)
        {
            return ResponseResult.CreateError($"Could not find a payroll with ID {req.Id}");
        }
        
        _tenantUow.Repository<Payroll>().Delete(payroll);
        await _tenantUow.SaveChangesAsync();
        return ResponseResult.CreateSuccess();
    }
}
