using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Common;
using Logistics.Application.Services;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Payments.Stripe;

namespace Logistics.Application.Queries;

// CQS violation accepted: read syncs cached Stripe Connect status into the master DB after
// each external lookup. Auto-transaction would wrap the Stripe API call inside a DB tx.
[NoAutoTransaction]
internal sealed class GetConnectStatusHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IStripeConnectService stripeConnectService,
    ILogger<GetConnectStatusHandler> logger)
    : IAppRequestHandler<GetConnectStatusQuery, Result<StripeConnectStatusDto>>
{
    public async Task<Result<StripeConnectStatusDto>> Handle(GetConnectStatusQuery req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();

        // If no connected account, return not connected status
        if (string.IsNullOrEmpty(tenant.StripeConnectedAccountId))
        {
            return Result<StripeConnectStatusDto>.Ok(new StripeConnectStatusDto
            {
                AccountId = null,
                Status = StripeConnectStatus.NotConnected,
                ChargesEnabled = false,
                PayoutsEnabled = false
            });
        }

        try
        {
            // Sync the latest status from Stripe
            await stripeConnectService.SyncConnectedAccountStatusAsync(tenant);

            // Save the updated status
            masterUow.Repository<Domain.Entities.Tenant>().Update(tenant);
            await masterUow.SaveChangesAsync(ct);

            return Result<StripeConnectStatusDto>.Ok(new StripeConnectStatusDto
            {
                AccountId = tenant.StripeConnectedAccountId,
                Status = tenant.ConnectStatus,
                ChargesEnabled = tenant.ChargesEnabled,
                PayoutsEnabled = tenant.PayoutsEnabled
            });
        }
        catch (Stripe.StripeException ex)
        {
            logger.LogError(ex, "Failed to get Connect status for tenant {TenantId}", tenant.Id);

            // Return cached status if Stripe call fails
            return Result<StripeConnectStatusDto>.Ok(new StripeConnectStatusDto
            {
                AccountId = tenant.StripeConnectedAccountId,
                Status = tenant.ConnectStatus,
                ChargesEnabled = tenant.ChargesEnabled,
                PayoutsEnabled = tenant.PayoutsEnabled
            });
        }
    }
}
