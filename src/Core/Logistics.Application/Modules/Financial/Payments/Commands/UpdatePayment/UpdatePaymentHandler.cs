using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.Application.Modules.Financial.Payments.Commands;

internal sealed class UpdatePaymentHandler(ITenantUnitOfWork tenantUow)
    : UpdateTenantEntityHandler<UpdatePaymentCommand, Payment>(tenantUow)
{
    protected override void ApplyChanges(UpdatePaymentCommand req, Payment payment)
    {
        if (req.StripePaymentMethodId is not null)
        {
            payment.StripePaymentMethodId = req.StripePaymentMethodId;
        }

        payment.Status = PropertyUpdater.UpdateIfChanged(req.Status, payment.Status);
        if (req.Amount is not null && req.Amount != payment.Amount.Amount)
        {
            payment.Amount = new() { Amount = req.Amount.Value, Currency = payment.Amount.Currency };
        }

        payment.BillingAddress = PropertyUpdater.UpdateIfChanged(req.BillingAddress, payment.BillingAddress);
        payment.Description = PropertyUpdater.UpdateIfChanged(req.Description, payment.Description);
    }
}
