namespace Logistics.Application.Tenant.Commands;

internal sealed class DeletePaymentHandler : RequestHandler<DeletePaymentCommand, ResponseResult>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public DeletePaymentHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<ResponseResult> HandleValidated(
        DeletePaymentCommand req, CancellationToken cancellationToken)
    {
        var payment = await _tenantUow.Repository<Payment>().GetByIdAsync(req.Id);

        if (payment is null)
        {
            return ResponseResult.CreateError($"Could not find a payment with ID {req.Id}");
        }
            
        
        _tenantUow.Repository<Payment>().Delete(payment);
        await _tenantUow.SaveChangesAsync();
        return ResponseResult.CreateSuccess();
    }
}
