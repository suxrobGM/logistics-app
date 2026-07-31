using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe.Billing;
using Logistics.Application.Abstractions.SystemSettings;
using Logistics.Application.Abstractions.Payments.Stripe;

namespace Logistics.Infrastructure.Payments.Stripe;

internal sealed class StripeUsageService(
    IMasterUnitOfWork masterUow,
    ISystemSettingsService settingService,
    IOptions<StripeOptions> options,
    ILogger<StripeUsageService> logger) : IStripeUsageService
{

    public async Task ReportAISessionOverageAsync(Guid tenantId, decimal sessionCostUsd, CancellationToken ct = default)
    {
        var meterId = await settingService.GetAsync(StripeSettingKeys.AIOverageMeterId, ct);
        if (string.IsNullOrEmpty(meterId))
        {
            logger.LogWarning(
                "AI overage meter not configured in system settings - overage for tenant {TenantId} will not be billed",
                tenantId);
            return;
        }

        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(tenantId, ct);

        if (tenant is null || !AIOverageBilling.CanBill(tenant))
        {
            logger.LogWarning("Cannot report AI overage: tenant {TenantId} has no subscription or Stripe customer",
                tenantId);
            return;
        }

        var billingUnits = AIOverageBilling.UnitsFor(sessionCostUsd);

        var meterEventService = new MeterEventService();
        await meterEventService.CreateAsync(new MeterEventCreateOptions
        {
            EventName = options.Value.AIOverageMeterEventName,
            Payload = new Dictionary<string, string>
            {
                // Non-null by CanBill above.
                ["stripe_customer_id"] = tenant.StripeCustomerId!,
                ["value"] = billingUnits.ToString()
            }
        }, cancellationToken: ct);

        logger.LogInformation(
            "Reported AI session overage meter event for tenant {TenantId}: ${CostUsd} model cost -> {Units} unit(s)",
            tenantId, sessionCostUsd, billingUnits);
    }
}
