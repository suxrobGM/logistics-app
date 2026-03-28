namespace Logistics.Infrastructure.Payments.Stripe;

public class StripeOptions
{
    public const string SectionName = "Stripe";
    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }

    /// <summary>
    /// Stripe Billing Meter event name for AI dispatch overages.
    /// Must match the meter's event_name configuration in Stripe.
    /// The meter ID itself is stored in the database (SystemSetting).
    /// </summary>
    public string AiOverageMeterEventName { get; set; } = "ai_dispatch_session";
}
