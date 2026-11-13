using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Payments;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Financial.StripeConnect.Commands;

internal sealed class CreateConnectAccountHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IStripeConnectService stripeConnectService,
    ILogger<CreateConnectAccountHandler> logger)
    : IRequestHandler<CreateConnectAccountCommand, Result<CreateConnectAccountDto>>
{
    public async Task<Result<CreateConnectAccountDto>> Handle(CreateConnectAccountCommand req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();

        // Check if already has a connected account
        if (!string.IsNullOrEmpty(tenant.StripeConnectedAccountId))
        {
            logger.LogInformation(
                "Tenant {TenantId} already has Connect account {AccountId}",
                tenant.Id, tenant.StripeConnectedAccountId);
            return Result<CreateConnectAccountDto>.Ok(new CreateConnectAccountDto
            {
                AccountId = tenant.StripeConnectedAccountId
            });
        }

        try
        {
            // Create the connected account in Stripe
            var accountId = await stripeConnectService.CreateConnectedAccountAsync(tenant);

            // Update tenant with the connected account ID
            tenant.StripeConnectedAccountId = accountId;
            tenant.ConnectStatus = Domain.Primitives.Enums.StripeConnectStatus.Pending;

            await masterUow.SaveChangesAsync(ct);

            logger.LogInformation(
                "Created Stripe Connect account {AccountId} for tenant {TenantId}",
                accountId, tenant.Id);

            return Result<CreateConnectAccountDto>.Ok(new CreateConnectAccountDto
            {
                AccountId = accountId
            });
        }
        catch (StripeApiException ex)
        {
            logger.LogError(ex, "Failed to create Stripe Connect account for tenant {TenantId}", tenant.Id);
            return Result<CreateConnectAccountDto>.Fail($"Failed to create Stripe Connect account: {ex.Message}");
        }
    }
}
