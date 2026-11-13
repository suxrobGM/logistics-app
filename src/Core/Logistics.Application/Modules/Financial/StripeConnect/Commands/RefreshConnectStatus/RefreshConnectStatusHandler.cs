using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Payments;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Financial.StripeConnect.Commands;

internal sealed class RefreshConnectStatusHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IStripeConnectService stripeConnectService,
    ILogger<RefreshConnectStatusHandler> logger)
    : IRequestHandler<RefreshConnectStatusCommand, Result<StripeConnectStatusDto>>
{
    public async Task<Result<StripeConnectStatusDto>> Handle(RefreshConnectStatusCommand req, CancellationToken ct)
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

            await masterUow.SaveChangesAsync(ct);

            return Result<StripeConnectStatusDto>.Ok(new StripeConnectStatusDto
            {
                AccountId = tenant.StripeConnectedAccountId,
                Status = tenant.ConnectStatus,
                ChargesEnabled = tenant.ChargesEnabled,
                PayoutsEnabled = tenant.PayoutsEnabled
            });
        }
        catch (StripeApiException ex)
        {
            logger.LogError(ex, "Failed to refresh Connect status for tenant {TenantId}", tenant.Id);

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
