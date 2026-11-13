namespace Logistics.Application.Abstractions.Payments;

public sealed record StripeCheckoutLink(string SessionId, string Url);
