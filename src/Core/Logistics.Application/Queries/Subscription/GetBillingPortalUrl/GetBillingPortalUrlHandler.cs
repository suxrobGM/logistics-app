using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetBillingPortalUrlHandler(
    IStripePortalService stripePortalService,
    ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetBillingPortalUrlQuery, Result<BillingPortalUrlDto>>
{
    public async Task<Result<BillingPortalUrlDto>> Handle(
        GetBillingPortalUrlQuery req, CancellationToken ct)
    {
        var tenant = tenantUow.GetCurrentTenant();

        if (string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            return Result<BillingPortalUrlDto>.Fail("Tenant does not have a Stripe customer ID.");
        }

        var portalUrl = await stripePortalService.CreateBillingPortalSessionAsync(
            tenant.StripeCustomerId, req.ReturnUrl, ct);

        return Result<BillingPortalUrlDto>.Ok(new BillingPortalUrlDto { Url = portalUrl });
    }
}
