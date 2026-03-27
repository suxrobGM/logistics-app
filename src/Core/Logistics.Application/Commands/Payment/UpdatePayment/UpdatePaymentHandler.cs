using Logistics.Application.Abstractions;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdatePaymentHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<UpdatePaymentCommand, Result>
{
    public async Task<Result> Handle(
        UpdatePaymentCommand req, CancellationToken ct)
    {
        var payment = await tenantUow.Repository<Payment>().GetByIdAsync(req.Id);

        if (payment is null)
        {
            return Result.Fail($"Could not find a payment with ID '{req.Id}'");
        }

        if (req.StripePaymentMethodId is not null)
        {
            payment.StripePaymentMethodId = req.StripePaymentMethodId;
        }

        payment.Status = PropertyUpdater.UpdateIfChanged(req.Status, payment.Status);
        payment.Amount = PropertyUpdater.UpdateIfChanged(req.Amount, payment.Amount.Amount);
        payment.BillingAddress = PropertyUpdater.UpdateIfChanged(req.BillingAddress, payment.BillingAddress);
        payment.Description = PropertyUpdater.UpdateIfChanged(req.Description, payment.Description);

        tenantUow.Repository<Payment>().Update(payment);
        await tenantUow.SaveChangesAsync();
        return Result.Ok();
    }
}
