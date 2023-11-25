using Logistics.Shared.Enums;

namespace Logistics.Application.Tenant.Commands;

internal sealed class ProcessPaymentHandler : RequestHandler<ProcessPaymentCommand, ResponseResult>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public ProcessPaymentHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<ResponseResult> HandleValidated(
        ProcessPaymentCommand req, CancellationToken cancellationToken)
    {
        var payment = await _tenantUow.Repository<Payment>().GetByIdAsync(req.PaymentId);

        if (payment is null)
        {
            return ResponseResult.CreateError($"Could not find a payment with ID '{req.PaymentId}'");
        }

        // TODO: Add payment verification from external provider
        payment.SetStatus(PaymentStatus.Paid);
        _tenantUow.Repository<Payment>().Update(payment);
        await _tenantUow.SaveChangesAsync();
        return ResponseResult.CreateSuccess();
    }
}
