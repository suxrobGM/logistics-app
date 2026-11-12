namespace Logistics.Application.Abstractions.Payments.Stripe;

/// <param name="Status">Stripe's raw status, for example <c>succeeded</c> or <c>requires_action</c>.</param>
public sealed record StripePaymentIntentInfo(
    string Id,
    string Status,
    string? LastErrorMessage,
    IReadOnlyDictionary<string, string> Metadata);
