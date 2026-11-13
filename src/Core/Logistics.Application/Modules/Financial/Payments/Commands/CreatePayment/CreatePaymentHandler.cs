using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payments.Commands;

internal sealed class CreatePaymentHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<CreatePaymentCommand, Result>
{
    public async Task<Result> Handle(
        CreatePaymentCommand req, CancellationToken ct)
    {
        var payment = new Payment
        {
            Amount = req.Amount,
            StripePaymentMethodId = req.StripePaymentMethodId,
            BillingAddress = req.BillingAddress!,
            Description = req.Description
        };

        await tenantUow.Repository<Payment>().AddAsync(payment);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
