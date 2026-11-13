namespace Logistics.Application.Abstractions.Payments;

public sealed record StripeAccountLink(string Url, DateTime ExpiresAt);
