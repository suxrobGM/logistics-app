namespace Logistics.Application.Services;

/// <summary>
/// Reports metered usage to Stripe for usage-based billing (e.g., AI session overages).
/// </summary>
public interface IStripeUsageService
{
    /// <summary>
    /// Reports AI session usage overage for a tenant to Stripe. Converts billing units to Stripe's expected format.
    /// Billing units are determined by the model used (e.g., base=1, premium=2, ultra=4) and multiplied by the number of sessions to calculate total usage.
    /// </summary>
    /// <param name="tenantId">The tenant for which to report usage.</param>
    /// <param name="billingUnits">
    /// The number of billing units to report (e.g., 1 for base model, 2 for premium, etc.).
    /// This should be calculated based on the model used and the number of sessions.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ReportAiSessionOverageAsync(
        Guid tenantId,
        int billingUnits = 1,
        CancellationToken ct = default);
}
