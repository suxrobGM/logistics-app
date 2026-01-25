using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Queries;

internal sealed class GetDashboardLinkHandler(
    ITenantUnitOfWork tenantUow,
    IStripeConnectService stripeConnectService,
    ILogger<GetDashboardLinkHandler> logger)
    : IAppRequestHandler<GetDashboardLinkQuery, Result<DashboardLinkDto>>
{
    public async Task<Result<DashboardLinkDto>> Handle(GetDashboardLinkQuery req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();

        if (string.IsNullOrEmpty(tenant.StripeConnectedAccountId))
        {
            return Result<DashboardLinkDto>.Fail("Tenant does not have a Stripe Connect account.");
        }

        try
        {
            var loginLink = await stripeConnectService.CreateLoginLinkAsync(tenant.StripeConnectedAccountId);

            logger.LogInformation(
                "Created dashboard link for tenant {TenantId}",
                tenant.Id);

            return Result<DashboardLinkDto>.Ok(new DashboardLinkDto
            {
                Url = loginLink.Url
            });
        }
        catch (Stripe.StripeException ex)
        {
            logger.LogError(ex, "Failed to create dashboard link for tenant {TenantId}", tenant.Id);
            return Result<DashboardLinkDto>.Fail($"Failed to create dashboard link: {ex.Message}");
        }
    }
}
