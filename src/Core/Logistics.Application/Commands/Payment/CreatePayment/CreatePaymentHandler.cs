using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreatePaymentHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<CreatePaymentCommand, Result>
{
    public async Task<Result> Handle(
        CreatePaymentCommand req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();

        var payment = new Payment
        {
            Amount = req.Amount,
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
