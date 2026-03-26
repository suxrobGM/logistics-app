namespace Logistics.Infrastructure.Payments.Stripe;

public class StripeOptions
{
    public const string SectionName = "Stripe";
    public string? PublishableKey { get; set; }
    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }

    /// <summary>
    /// Stripe Billing Meter ID for AI dispatch session overages.
    /// Created once in Stripe Dashboard or via API, shared across all plans.
    /// </summary>
    public string? AiOverageMeterId { get; set; }

    /// <summary>
    /// Stripe Billing Meter event name for AI dispatch overages.
    /// Must match the meter's event_name configuration.
    /// </summary>
    public string AiOverageMeterEventName { get; set; } = "ai_dispatch_session";
}
