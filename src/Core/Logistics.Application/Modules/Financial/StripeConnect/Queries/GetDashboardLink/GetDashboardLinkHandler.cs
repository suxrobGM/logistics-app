using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Payments;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Financial.StripeConnect.Queries;

internal sealed class GetDashboardLinkHandler(
    ITenantUnitOfWork tenantUow,
    IStripeConnectService stripeConnectService,
    ILogger<GetDashboardLinkHandler> logger)
    : IRequestHandler<GetDashboardLinkQuery, Result<DashboardLinkDto>>
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
            var dashboardUrl = await stripeConnectService.CreateLoginLinkAsync(tenant.StripeConnectedAccountId);

            logger.LogInformation(
                "Created dashboard link for tenant {TenantId}",
                tenant.Id);

            return Result<DashboardLinkDto>.Ok(new DashboardLinkDto
            {
                Url = dashboardUrl
            });
        }
        catch (StripeApiException ex)
        {
            logger.LogError(ex, "Failed to create dashboard link for tenant {TenantId}", tenant.Id);
            return Result<DashboardLinkDto>.Fail($"Failed to create dashboard link: {ex.Message}");
        }
    }
}
