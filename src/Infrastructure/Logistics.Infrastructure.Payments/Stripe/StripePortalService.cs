using Microsoft.Extensions.Logging;
using Stripe.BillingPortal;
using Logistics.Application.Abstractions.Payments.Stripe;

namespace Logistics.Infrastructure.Payments.Stripe;

internal sealed class StripePortalService(ILogger<StripePortalService> logger) : IStripePortalService
{
    public async Task<string> CreateBillingPortalSessionAsync(
        string stripeCustomerId, string returnUrl, CancellationToken ct = default)
    {
        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(new SessionCreateOptions
        {
            Customer = stripeCustomerId,
            ReturnUrl = returnUrl
        }, cancellationToken: ct);

        logger.LogInformation("Created billing portal session for customer {CustomerId}", stripeCustomerId);
        return session.Url;
    }
}
