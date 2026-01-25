using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class CreateConnectAccountHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IStripeConnectService stripeConnectService,
    ILogger<CreateConnectAccountHandler> logger)
    : IAppRequestHandler<CreateConnectAccountCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CreateConnectAccountCommand req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();

        // Check if already has a connected account
        if (!string.IsNullOrEmpty(tenant.StripeConnectedAccountId))
        {
            logger.LogInformation(
                "Tenant {TenantId} already has Connect account {AccountId}",
                tenant.Id, tenant.StripeConnectedAccountId);
            return Result<string>.Ok(tenant.StripeConnectedAccountId);
        }

        try
        {
            // Create the connected account in Stripe
            var account = await stripeConnectService.CreateConnectedAccountAsync(tenant);

            // Update tenant with the connected account ID
            tenant.StripeConnectedAccountId = account.Id;
            tenant.ConnectStatus = Domain.Primitives.Enums.StripeConnectStatus.Pending;

            masterUow.Repository<Domain.Entities.Tenant>().Update(tenant);
            await masterUow.SaveChangesAsync(ct);

            logger.LogInformation(
                "Created Stripe Connect account {AccountId} for tenant {TenantId}",
                account.Id, tenant.Id);

            return Result<string>.Ok(account.Id);
        }
        catch (Stripe.StripeException ex)
        {
            logger.LogError(ex, "Failed to create Stripe Connect account for tenant {TenantId}", tenant.Id);
            return Result<string>.Fail($"Failed to create Stripe Connect account: {ex.Message}");
        }
    }
}
