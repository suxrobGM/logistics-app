using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

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
        var paymentMethod = await _tenantUow.Repository<PaymentMethod>().GetByIdAsync(req.PaymentMethodId);
        
        if (paymentMethod is null)
        {
            return Result.Fail($"Could not find a payment method with ID '{req.PaymentMethodId}'");
        }
        
        var tenant = _tenantUow.GetCurrentTenant();
        
        var payment = new Payment
        {
            Amount = req.Amount,
            MethodId = paymentMethod.Id,
            TenantId = tenant.Id,
            BillingAddress = req.BillingAddress!,
            Description = req.Description,
        };
        
        await _tenantUow.Repository<Payment>().AddAsync(payment);
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
