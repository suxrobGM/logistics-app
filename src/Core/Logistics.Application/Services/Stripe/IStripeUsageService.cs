namespace Logistics.Application.Services;

/// <summary>
/// Reports metered usage to Stripe for usage-based billing (e.g., AI session overages).
/// </summary>
public interface IStripeUsageService
{
    Task ReportAiSessionOverageAsync(Guid tenantId, CancellationToken ct = default);
}
