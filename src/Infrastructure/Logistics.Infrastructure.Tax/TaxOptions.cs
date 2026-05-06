namespace Logistics.Infrastructure.Tax;

public class TaxOptions
{
    public const string SectionName = "Tax";

    /// <summary>
    /// "stripe" (default) routes to <c>StripeTaxCalculator</c> via <c>Stripe.TaxCalculationService</c>;
    /// "manual" uses <c>ManualTaxCalculator</c> against tenant-managed rates with EU/US/UK fallback tables.
    /// </summary>
    public string Provider { get; set; } = "stripe";

    /// <summary>
    /// Last-resort Stripe Tax product code used when neither the line item nor the tenant's
    /// Stripe Tax default settings supply one. txcd_99999999 = generic taxable goods/services.
    /// </summary>
    public string FallbackTaxCode { get; set; } = "txcd_99999999";

    /// <summary>
    /// TTL for the cached tenant Stripe Tax settings/registrations.
    /// </summary>
    public int StripeConfigCacheMinutes { get; set; } = 15;
}
