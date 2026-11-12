namespace Logistics.Application.Abstractions.Payments.Stripe;

public sealed record StripeAccountLink(string Url, DateTime ExpiresAt);
