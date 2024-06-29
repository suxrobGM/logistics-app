using Logistics.Domain.ValueObjects;

namespace Logistics.Application.Tenant.Commands;

internal sealed class CreatePaymentHandler : RequestHandler<CreatePaymentCommand, Result>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public CreatePaymentHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<Result> HandleValidated(
        CreatePaymentCommand req, CancellationToken cancellationToken)
    {
        var payment = new Payment
        {
            Amount = req.Amount,
            Method = req.Method,
            PaymentFor = req.PaymentFor,
            BillingAddress = req.BillingAddress ?? Address.NullAddress,
            Comment = req.Comment
        };
        
        await _tenantUow.Repository<Payment>().AddAsync(payment);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
