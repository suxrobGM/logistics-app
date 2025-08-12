using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreatePaymentHandler : IAppRequestHandler<CreatePaymentCommand, Result>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public CreatePaymentHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public async Task<Result> Handle(
        CreatePaymentCommand req, CancellationToken ct)
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
            Description = req.Description
        };

        await _tenantUow.Repository<Payment>().AddAsync(payment);
        await _tenantUow.SaveChangesAsync();
        return Result.Ok();
    }
}
