namespace Logistics.Application.Abstractions.Payments;

/// <summary>Exactly one payload property is set, matching <see cref="Kind"/>; none for <see cref="StripeWebhookEventKind.Other"/>.</summary>
public sealed record StripeWebhookEvent
{
    public required string Id { get; init; }

    /// <summary>Stripe's event type, for example <c>customer.subscription.updated</c>.</summary>
    public required string Type { get; init; }

    public required StripeWebhookEventKind Kind { get; init; }
    public StripeSubscriptionInfo? Subscription { get; init; }
    public StripePaymentIntentInfo? PaymentIntent { get; init; }
    public StripeAccountInfo? Account { get; init; }
    public StripeCheckoutSessionInfo? CheckoutSession { get; init; }
}
