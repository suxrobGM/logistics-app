namespace Logistics.Application.Abstractions.Payments.Stripe;

public sealed record StripeCheckoutSessionInfo
{
    public required string Id { get; init; }
    public string? PaymentIntentId { get; init; }

    /// <summary>Stripe's raw payment status: <c>paid</c>, <c>unpaid</c> or <c>no_payment_required</c>.</summary>
    public string? PaymentStatus { get; init; }

    /// <summary>Total in major units, already converted from Stripe's minor units.</summary>
    public decimal AmountTotal { get; init; }

    public string? Currency { get; init; }
    public required IReadOnlyDictionary<string, string> Metadata { get; init; }
}
