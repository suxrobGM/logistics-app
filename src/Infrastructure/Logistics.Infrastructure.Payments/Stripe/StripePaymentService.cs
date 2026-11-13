using Logistics.Application.Abstractions.Payments;
using Logistics.Domain.Entities;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Logistics.Infrastructure.Payments.Stripe;

internal sealed class StripePaymentService(ILogger<StripePaymentService> logger) : IStripePaymentService
{
    public async Task<string> CreateSetupIntentAsync(Tenant tenant)
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
        logger.LogInformation("Created SetupIntent for tenant {TenantId}", tenant.Id);
        return setupIntent.ClientSecret;
    }

    public async Task<StripePaymentIntentInfo> CreatePaymentIntentAsync(Payment payment, Tenant tenant)
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
            Amount = StripeMoneyConversion.ToMinorUnits(payment.Amount, tenant.Currency),
            Currency = tenant.Currency.ToLowerInvariant(),
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

        var paymentIntent = await StripeApi.CallAsync(() => new PaymentIntentService().CreateAsync(createOptions));
        logger.LogInformation("Created PaymentIntent {PaymentIntentId} for tenant {TenantId}",
            paymentIntent.Id, tenant.Id);
        return paymentIntent.ToInfo();
    }
}
