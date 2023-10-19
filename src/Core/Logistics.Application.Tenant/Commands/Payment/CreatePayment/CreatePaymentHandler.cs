namespace Logistics.Application.Tenant.Commands;

internal sealed class CreatePaymentHandler : RequestHandler<CreatePaymentCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public CreatePaymentHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        CreatePaymentCommand req, CancellationToken cancellationToken)
    {
        var payment = new Payment
        {
            Amount = req.Amount,
            Method = req.Method,
            PaymentFor = req.PaymentFor,
            Comment = req.Comment
        };
        
        await _tenantRepository.AddAsync(payment);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}
