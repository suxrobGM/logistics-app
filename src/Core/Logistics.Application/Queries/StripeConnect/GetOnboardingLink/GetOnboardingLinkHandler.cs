using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Queries;

internal sealed class GetOnboardingLinkHandler(
    ITenantUnitOfWork tenantUow,
    IStripeConnectService stripeConnectService,
    ILogger<GetOnboardingLinkHandler> logger)
    : IAppRequestHandler<GetOnboardingLinkQuery, Result<string>>
{
    public async Task<Result<string>> Handle(GetOnboardingLinkQuery req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();

        if (string.IsNullOrEmpty(tenant.StripeConnectedAccountId))
        {
            return Result<string>.Fail("Tenant does not have a Stripe Connect account. Create one first.");
        }

        try
        {
            var accountLink = await stripeConnectService.CreateAccountLinkAsync(
                tenant, req.ReturnUrl, req.RefreshUrl);

            logger.LogInformation(
                "Created onboarding link for tenant {TenantId}, expires at {ExpiresAt}",
                tenant.Id, accountLink.ExpiresAt);

            return Result<string>.Ok(accountLink.Url);
        }
        catch (Stripe.StripeException ex)
        {
            logger.LogError(ex, "Failed to create onboarding link for tenant {TenantId}", tenant.Id);
            return Result<string>.Fail($"Failed to create onboarding link: {ex.Message}");
        }
    }
}
