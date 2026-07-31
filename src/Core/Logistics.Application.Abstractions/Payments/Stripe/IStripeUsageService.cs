namespace Logistics.Application.Abstractions.Payments.Stripe;

/// <summary>
/// Reports metered usage to Stripe for usage-based billing (e.g., AI session overages).
/// </summary>
public interface IStripeUsageService
{
    /// <summary>
    /// Reports one over-budget AI session. Callers pass raw estimated model cost; the billing
    /// layer converts it to metered units (markup, rounding).
    /// </summary>
    Task ReportAISessionOverageAsync(
        Guid tenantId,
        decimal sessionCostUsd,
        CancellationToken ct = default);
}
