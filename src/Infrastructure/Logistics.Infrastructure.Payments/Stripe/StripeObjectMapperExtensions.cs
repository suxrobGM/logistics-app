using Logistics.Application.Abstractions.Payments.Stripe;
using Stripe;
using Stripe.Checkout;

namespace Logistics.Infrastructure.Payments.Stripe;

internal static class StripeObjectMapperExtensions
{
    extension(Subscription subscription)
    {
        public StripeSubscriptionInfo ToInfo() =>
            new(subscription.Id, subscription.Status, subscription.CancelAtPeriodEnd, subscription.Metadata);
    }

    extension(PaymentIntent paymentIntent)
    {
        public StripePaymentIntentInfo ToInfo() =>
            new(paymentIntent.Id, paymentIntent.Status, paymentIntent.LastPaymentError?.Message,
                paymentIntent.Metadata);
    }

    extension(Account account)
    {
        public StripeAccountInfo ToInfo() => new(account.Id, account.Metadata);
    }

    extension(Session session)
    {
        public StripeCheckoutSessionInfo ToInfo() => new()
        {
            Id = session.Id,
            PaymentIntentId = session.PaymentIntentId,
            PaymentStatus = session.PaymentStatus,
            AmountTotal = StripeMoneyConversion.FromMinorUnits(session.AmountTotal ?? 0, session.Currency ?? ""),
            Currency = session.Currency,
            Metadata = session.Metadata
        };
    }
}
