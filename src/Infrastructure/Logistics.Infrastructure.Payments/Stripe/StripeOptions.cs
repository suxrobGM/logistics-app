namespace Logistics.Infrastructure.Payments.Stripe;

public class StripeOptions
{
    public const string SectionName = "Stripe";
    public string? PublishableKey { get; set; }
    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }
}
