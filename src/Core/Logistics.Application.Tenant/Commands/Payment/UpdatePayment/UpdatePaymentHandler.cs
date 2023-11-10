namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdatePaymentHandler : RequestHandler<UpdatePaymentCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public UpdatePaymentHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        UpdatePaymentCommand req, CancellationToken cancellationToken)
    {
        var payment = await _tenantRepository.GetAsync<Payment>(req.Id);

        if (payment is null)
            return ResponseResult.CreateError($"Could not find a payment with ID '{req.Id}'");

        if (req.PaymentFor.HasValue && payment.PaymentFor != req.PaymentFor)
        {
            payment.PaymentFor = req.PaymentFor.Value;
        }
        if (req.Method.HasValue && payment.Method != req.Method)
        {
            payment.Method = req.Method.Value;
        }
        if (req.Status.HasValue && payment.Status != req.Status)
        {
            payment.SetStatus(req.Status.Value);
        }
        if (req.Amount.HasValue && payment.Amount != req.Amount)
        {
            payment.Amount = req.Amount.Value;
        }
        if (!string.IsNullOrEmpty(req.BillingAddress) && payment.BillingAddress != req.BillingAddress)
        {
            payment.BillingAddress = req.BillingAddress;
        }
        if (!string.IsNullOrEmpty(req.Comment) && payment.Comment != req.Comment)
        {
            payment.Comment = req.Comment;
        }
        
        _tenantRepository.Update(payment);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
