using Logistics.Shared.Enums;

namespace Logistics.Application.Tenant.Commands;

internal sealed class ProcessPaymentHandler : RequestHandler<ProcessPaymentCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public ProcessPaymentHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        ProcessPaymentCommand req, CancellationToken cancellationToken)
    {
        var payment = await _tenantRepository.GetAsync<Payment>(req.PaymentId);

        if (payment is null)
        {
            return ResponseResult.CreateError($"Could not find a payment with ID '{req.PaymentId}'");
        }

        // TODO: Add payment verification from external provider
        payment.SetStatus(PaymentStatus.Paid);
        _tenantRepository.Update(payment);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
