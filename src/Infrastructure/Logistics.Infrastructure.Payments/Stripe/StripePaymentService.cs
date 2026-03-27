using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace Logistics.Infrastructure.Payments.Stripe;

internal class StripePaymentService(IOptions<StripeOptions> options, ILogger<StripePaymentService> logger)
    : StripeServiceBase(options, logger), IStripePaymentService
{
    public async Task<SetupIntent> CreateSetupIntentAsync(Tenant tenant)
    {
        if (string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            throw new ArgumentException("Tenant must have a StripeCustomerId");
        }

        var createOptions = new SetupIntentCreateOptions
        {
            Customer = tenant.StripeCustomerId,
            Usage = "off_session",
            AutomaticPaymentMethods = new SetupIntentAutomaticPaymentMethodsOptions
            {
                Enabled = true
            },
            Metadata = new Dictionary<string, string>
            {
                [StripeMetadataKeys.TenantId] = tenant.Id.ToString()
            }
        };

        var setupIntentService = new SetupIntentService();
        var setupIntent = await setupIntentService.CreateAsync(createOptions);
        Logger.LogInformation("Created SetupIntent for tenant {TenantId}", tenant.Id);
        return setupIntent;
    }

    public async Task<PaymentIntent> CreatePaymentIntentAsync(Payment payment, Tenant tenant)
    {
        if (string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            throw new ArgumentException("Tenant must have a StripeCustomerId");
        }

        if (string.IsNullOrEmpty(payment.StripePaymentMethodId))
        {
            throw new ArgumentException("Payment must have a StripePaymentMethodId");
        }

        var createOptions = new PaymentIntentCreateOptions
        {
            Amount = (long)(payment.Amount.Amount * 100), // Convert to cents
            Currency = payment.Amount.Currency.ToLower(),
            Customer = tenant.StripeCustomerId,
            PaymentMethod = payment.StripePaymentMethodId,
            Confirm = true,
            OffSession = true,
            Description = payment.Description,
            Metadata = new Dictionary<string, string>
            {
                [StripeMetadataKeys.TenantId] = tenant.Id.ToString(),
                ["PaymentId"] = payment.Id.ToString()
            }
        };

        var paymentIntent = await new PaymentIntentService().CreateAsync(createOptions);
        Logger.LogInformation("Created PaymentIntent {PaymentIntentId} for tenant {TenantId}",
            paymentIntent.Id, tenant.Id);
        return paymentIntent;
    }

    public Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId)
    {
        return new PaymentIntentService().GetAsync(paymentIntentId);
    }
}
