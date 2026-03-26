using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe.Billing;

namespace Logistics.Infrastructure.Payments.Stripe;

internal sealed class StripeUsageService(
    IMasterUnitOfWork masterUow,
    ISystemSettingService settingService,
    IOptions<StripeOptions> options,
    ILogger<StripeUsageService> logger) : IStripeUsageService
{
    private const string MeterSettingKey = "Stripe:AiOverageMeterId";

    public async Task ReportAiSessionOverageAsync(Guid tenantId, CancellationToken ct = default)
    {
        var meterId = await settingService.GetAsync(MeterSettingKey, ct);
        if (string.IsNullOrEmpty(meterId))
        {
            logger.LogDebug("AI overage meter not configured in system settings, skipping usage report");
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
            EventName = options.Value.AiOverageMeterEventName,
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
