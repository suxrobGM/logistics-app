using Logistics.Application.Abstractions.Payments.Stripe;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Logistics.Infrastructure.Payments.Stripe;

internal sealed class StripeWebhookParser(IOptions<StripeOptions> options, ILogger<StripeWebhookParser> logger)
    : IStripeWebhookParser
{
    private readonly string _webhookSecret = options.Value.WebhookSecret
                                             ?? throw new ArgumentNullException(nameof(options.Value.WebhookSecret));

    public StripeWebhookEvent? Parse(string requestBody, string signature)
    {
        Event stripeEvent;
        try
        {
            // An account on a newer Stripe API version must not break delivery.
            stripeEvent = EventUtility.ConstructEvent(
                requestBody, signature, _webhookSecret, throwOnApiVersionMismatch: false);
        }
        catch (StripeException ex)
        {
            logger.LogWarning("Stripe webhook signature verification failed: {Message}", ex.Message);
            return null;
        }

        var parsed = new StripeWebhookEvent
        {
            Id = stripeEvent.Id,
            Type = stripeEvent.Type,
            Kind = GetKind(stripeEvent.Type)
        };

        if (parsed.Kind == StripeWebhookEventKind.Other)
        {
            return parsed;
        }

        return stripeEvent.Data.Object switch
        {
            Subscription subscription => parsed with { Subscription = subscription.ToInfo() },
            PaymentIntent paymentIntent => parsed with { PaymentIntent = paymentIntent.ToInfo() },
            Account account => parsed with { Account = account.ToInfo() },
            Session session => parsed with { CheckoutSession = session.ToInfo() },
            var other => throw new InvalidOperationException(
                $"Stripe event {stripeEvent.Id} of type {stripeEvent.Type} carries an unexpected {other?.GetType().Name}.")
        };
    }

    private static StripeWebhookEventKind GetKind(string eventType) => eventType switch
    {
        EventTypes.CustomerSubscriptionCreated => StripeWebhookEventKind.SubscriptionCreated,
        EventTypes.CustomerSubscriptionUpdated => StripeWebhookEventKind.SubscriptionUpdated,
        EventTypes.CustomerSubscriptionDeleted => StripeWebhookEventKind.SubscriptionDeleted,
        EventTypes.PaymentIntentProcessing => StripeWebhookEventKind.PaymentIntentProcessing,
        EventTypes.PaymentIntentSucceeded => StripeWebhookEventKind.PaymentIntentSucceeded,
        EventTypes.PaymentIntentPaymentFailed => StripeWebhookEventKind.PaymentIntentFailed,
        EventTypes.AccountUpdated => StripeWebhookEventKind.AccountUpdated,
        EventTypes.CheckoutSessionCompleted => StripeWebhookEventKind.CheckoutSessionCompleted,
        _ => StripeWebhookEventKind.Other
    };
}
