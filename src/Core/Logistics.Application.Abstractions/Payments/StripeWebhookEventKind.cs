namespace Logistics.Application.Abstractions.Payments;

/// <summary>The Stripe event types the webhook acts on; everything else is <see cref="Other"/>.</summary>
public enum StripeWebhookEventKind
{
    Other,
    SubscriptionCreated,
    SubscriptionUpdated,
    SubscriptionDeleted,
    PaymentIntentProcessing,
    PaymentIntentSucceeded,
    PaymentIntentFailed,
    AccountUpdated,
    CheckoutSessionCompleted
}
