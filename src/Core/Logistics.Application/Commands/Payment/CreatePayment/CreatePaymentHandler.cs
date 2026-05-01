using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreatePaymentHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<CreatePaymentCommand, Result>
{
    public async Task<Result> Handle(
        CreatePaymentCommand req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var currency = (tenant.Settings?.Currency ?? CurrencyCode.USD).ToString();

        var payment = new Payment
        {
            Amount = new() { Amount = req.Amount, Currency = currency },
            StripePaymentMethodId = req.StripePaymentMethodId,
            TenantId = tenant.Id,
            BillingAddress = req.BillingAddress!,
            Description = req.Description
        };

        await tenantUow.Repository<Payment>().AddAsync(payment);
        await tenantUow.SaveChangesAsync();
        return Result.Ok();
    }
}
