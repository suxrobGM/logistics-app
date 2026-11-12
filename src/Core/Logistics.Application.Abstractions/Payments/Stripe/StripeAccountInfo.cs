namespace Logistics.Application.Abstractions.Payments.Stripe;

public sealed record StripeAccountInfo(string Id, IReadOnlyDictionary<string, string> Metadata);
