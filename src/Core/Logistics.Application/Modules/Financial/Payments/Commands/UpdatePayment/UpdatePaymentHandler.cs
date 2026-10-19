using Logistics.Application.Abstractions;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payments.Commands;

internal sealed class UpdatePaymentHandler(ITenantUnitOfWork tenantUow) : IRequestHandler<UpdatePaymentCommand, Result>
{
    public async Task<Result> Handle(UpdatePaymentCommand req, CancellationToken ct)
    {
        var payment = await tenantUow.Repository<Payment>().GetByIdAsync(req.Id, ct);
        if (payment is null)
        {
            return Result.Fail($"Could not find a {EntityName.Humanize<Payment>()} with ID '{req.Id}'");
        }

        if (req.StripePaymentMethodId is not null)
        {
            payment.StripePaymentMethodId = req.StripePaymentMethodId;
        }

        var statusChanged = req.Status is not null && req.Status != payment.Status;
        var amountChanged = req.Amount is not null && req.Amount != payment.Amount.Amount;

        payment.Status = PropertyUpdater.UpdateIfChanged(req.Status, payment.Status);
        payment.BillingAddress = PropertyUpdater.UpdateIfChanged(req.BillingAddress, payment.BillingAddress);
        payment.Description = PropertyUpdater.UpdateIfChanged(req.Description, payment.Description);

        if (amountChanged)
        {
            payment.Amount = new() { Amount = req.Amount!.Value, Currency = payment.Amount.Currency };
        }

        if (statusChanged || amountChanged)
        {
            payment.Invoice?.RecalculatePaymentStatus(DateTime.UtcNow);
        }

        tenantUow.Repository<Payment>().Update(payment);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
