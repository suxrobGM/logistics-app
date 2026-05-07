namespace Logistics.Shared.Models;

/// <summary>
/// Response from the public-payment Checkout Session endpoint. The customer is redirected
/// to <see cref="Url"/> to complete payment on Stripe's hosted page.
/// </summary>
public record PublicCheckoutSessionDto
{
    public required string Url { get; init; }
}
