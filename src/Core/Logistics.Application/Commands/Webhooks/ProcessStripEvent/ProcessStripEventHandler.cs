using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Consts;
using Logistics.Shared.Models;
using Stripe;
using Subscription = Logistics.Domain.Entities.Subscription;
using StripeInvoice = Stripe.Invoice;

namespace Logistics.Application.Commands;

internal sealed class ProcessStripEventHandler : RequestHandler<ProcessStripEventCommand, Result>
{
    private readonly IMasterUnityOfWork _masterUow;

    public ProcessStripEventHandler(IMasterUnityOfWork masterUow)
    {
        _masterUow = masterUow;
    }

    protected override async Task<Result> HandleValidated(
        ProcessStripEventCommand req, CancellationToken cancellationToken)
    {
        var stripeEvent = EventUtility.ConstructEvent(req.RequestBodyJson, req.StripeSignature, "your_webhook_secret");

        switch (stripeEvent.Type)
        {
            case EventTypes.InvoicePaid:
                var invoice = stripeEvent.Data.Object as StripeInvoice;
                return await HandleInvoicePaid(invoice!);
        }
        
        return Result.Succeed();
    }

    private async Task<Result> HandleInvoicePaid(StripeInvoice stripeInvoice)
    {
        var subsRepository = _masterUow.Repository<Subscription>();
        var subsPaymentRepository = _masterUow.Repository<SubscriptionPayment>();
        var subscription = await subsRepository.GetAsync(i => i.StripeSubscriptionId == stripeInvoice.Subscription.Id);

        if (subscription is null)
        {
            return Result.Fail($"Could not find a subscription with Stripe ID '{stripeInvoice.Subscription.Id}'");
        }
        
        var payment = new SubscriptionPayment
        {
            Amount = stripeInvoice.AmountPaid / 100m, // Convert from cents
            Status = PaymentStatus.Paid,
            PaymentDate = DateTime.UtcNow,
            StripeInvoiceId = stripeInvoice.Id,
            Subscription = subscription,
            SubscriptionId = subscription.Id
        };
        
        await subsPaymentRepository.AddAsync(payment);
        await _masterUow.SaveChangesAsync();
        return Result.Succeed();
    }
}
