using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Queries;

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
