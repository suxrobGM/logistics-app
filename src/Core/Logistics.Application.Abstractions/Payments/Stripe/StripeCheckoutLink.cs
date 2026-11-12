namespace Logistics.Application.Abstractions.Payments.Stripe;

public sealed record StripeCheckoutLink(string SessionId, string Url);
