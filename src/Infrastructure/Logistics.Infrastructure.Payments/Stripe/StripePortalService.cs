using Logistics.Application.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe.BillingPortal;

namespace Logistics.Infrastructure.Payments.Stripe;

internal sealed class StripePortalService(
    IOptions<StripeOptions> options,
    ILogger<StripePortalService> logger)
    : StripeServiceBase(options, logger), IStripePortalService
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

        Logger.LogInformation("Created billing portal session for customer {CustomerId}", stripeCustomerId);
        return session.Url;
    }
}
