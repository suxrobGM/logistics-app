namespace Logistics.Infrastructure.Payments.Stripe;

public class StripeOptions
{
    public const string SectionName = "Stripe";
    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }

    /// <summary>
    /// Stripe Billing Meter event name for AI agent overages (dispatch and copilot). The seeder
    /// resolves the meter by it and <c>StripeUsageService</c> emits it, so both read it here.
    /// Stripe fixes the name at meter creation: changing it builds a new meter and new prices.
    /// </summary>
    public string AIOverageMeterEventName { get; set; } = "ai_agent_session";
}
