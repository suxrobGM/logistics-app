using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe.Billing;

namespace Logistics.Infrastructure.Payments.Stripe;

internal sealed class StripeUsageService(
    IMasterUnitOfWork masterUow,
    IOptions<StripeOptions> options,
    ILogger<StripeUsageService> logger) : IStripeUsageService
{
    public async Task ReportAiSessionOverageAsync(Guid tenantId, CancellationToken ct = default)
    {
        var config = options.Value;

        if (string.IsNullOrEmpty(config.AiOverageMeterId))
        {
            logger.LogDebug("AI overage meter not configured, skipping usage report");
            return;
        }

        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(tenantId, ct);

        if (tenant?.Subscription is null || string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            logger.LogWarning("Cannot report AI overage: tenant {TenantId} has no subscription or Stripe customer",
                tenantId);
            return;
        }

        var meterEventService = new MeterEventService();
        await meterEventService.CreateAsync(new MeterEventCreateOptions
        {
            EventName = config.AiOverageMeterEventName,
            Payload = new Dictionary<string, string>
            {
                ["stripe_customer_id"] = tenant.StripeCustomerId,
                ["value"] = "1"
            }
        }, cancellationToken: ct);

        logger.LogInformation(
            "Reported AI session overage meter event for tenant {TenantId}", tenantId);
    }
}
