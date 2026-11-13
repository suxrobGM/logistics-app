namespace Logistics.Application.Abstractions.Payments;

/// <param name="Status">Stripe's raw status, for example <c>active</c> or <c>past_due</c>.</param>
public sealed record StripeSubscriptionInfo(
    string Id,
    string Status,
    bool CancelAtPeriodEnd,
    IReadOnlyDictionary<string, string> Metadata);
