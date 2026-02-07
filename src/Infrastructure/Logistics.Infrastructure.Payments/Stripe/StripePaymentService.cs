using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using PaymentMethod = Logistics.Domain.Entities.PaymentMethod;
using StripePaymentMethod = Stripe.PaymentMethod;

namespace Logistics.Infrastructure.Payments.Stripe;

internal class StripePaymentService(IOptions<StripeOptions> options, ILogger<StripePaymentService> logger)
    : StripeServiceBase(options, logger), IStripePaymentService
{
    #region Setup Intent API

    public async Task<SetupIntent> CreateSetupIntentAsync(Tenant tenant)
    {
        if (string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            throw new ArgumentException("Tenant must have a StripeCustomerId");
        }

        var options = new SetupIntentCreateOptions
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
        var setupIntent = await setupIntentService.CreateAsync(options);
        Logger.LogInformation("Created SetupIntent for tenant {TenantId}", tenant.Id);
        return setupIntent;
    }

    #endregion

    #region Payment Method API

    public async Task<StripePaymentMethod> UpdatePaymentMethodAsync(PaymentMethod paymentMethod)
    {
        if (string.IsNullOrEmpty(paymentMethod.StripePaymentMethodId))
        {
            throw new ArgumentException("Payment method must have a Stripe ID");
        }

        var service = new PaymentMethodService();
        var options = new PaymentMethodUpdateOptions
        {
            BillingDetails = CreateBillingDetails(paymentMethod)
        };

        switch (paymentMethod)
        {
            case CardPaymentMethod card:
                options.Card = MapCardPaymentMethod(card);
                break;
            case UsBankAccountPaymentMethod usBank:
                options.UsBankAccount = MapUsBankPaymentMethod(usBank);
                break;
        }

        var updatedMethod = await service.UpdateAsync(paymentMethod.StripePaymentMethodId, options);
        Logger.LogInformation("Updated Stripe payment method {PaymentMethodId}",
            paymentMethod.StripePaymentMethodId);
        return updatedMethod;
    }

    public async Task RemovePaymentMethodAsync(PaymentMethod paymentMethod)
    {
        if (string.IsNullOrEmpty(paymentMethod.StripePaymentMethodId))
        {
            throw new ArgumentException("Payment method must have a Stripe ID");
        }

        await new PaymentMethodService().DetachAsync(paymentMethod.StripePaymentMethodId);
        Logger.LogInformation("Removed Stripe payment method {PaymentMethodId}",
            paymentMethod.StripePaymentMethodId);
    }

    public async Task SetDefaultPaymentMethodAsync(PaymentMethod paymentMethod, Tenant tenant)
    {
        if (string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            throw new ArgumentException("Tenant must have a StripeCustomerId");
        }

        if (string.IsNullOrEmpty(paymentMethod.StripePaymentMethodId))
        {
            throw new ArgumentException("Payment method must have a StripePaymentMethodId");
        }

        await new CustomerService().UpdateAsync(tenant.StripeCustomerId, new CustomerUpdateOptions
        {
            InvoiceSettings = new CustomerInvoiceSettingsOptions
            {
                DefaultPaymentMethod = paymentMethod.StripePaymentMethodId
            }
        });

        Logger.LogInformation(
            "Set default Stripe payment method for tenant {TenantId}, Stripe payment method ID {StripePaymentMethodId}",
            tenant.Id, paymentMethod.StripePaymentMethodId);
    }

    public async Task<StripePaymentMethod> AttachPaymentMethodAsync(string stripePaymentMethodId, Tenant tenant)
    {
        if (string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            throw new ArgumentException("Tenant must have a StripeCustomerId");
        }

        var service = new PaymentMethodService();
        var attachedMethod = await service.AttachAsync(stripePaymentMethodId, new PaymentMethodAttachOptions
        {
            Customer = tenant.StripeCustomerId
        });

        Logger.LogInformation("Attached payment method {PaymentMethodId} to customer {CustomerId}",
            stripePaymentMethodId, tenant.StripeCustomerId);
        return attachedMethod;
    }

    public async Task<StripePaymentMethod?> GetPaymentMethodAsync(string stripePaymentMethodId)
    {
        try
        {
            return await new PaymentMethodService().GetAsync(stripePaymentMethodId);
        }
        catch (StripeException ex) when (ex.StripeError?.Code == "resource_missing")
        {
            return null;
        }
    }

    #endregion

    #region Payment Intent API

    public async Task<PaymentIntent> CreatePaymentIntentAsync(Payment payment, PaymentMethod paymentMethod,
        Tenant tenant)
    {
        if (string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            throw new ArgumentException("Tenant must have a StripeCustomerId");
        }

        if (string.IsNullOrEmpty(paymentMethod.StripePaymentMethodId))
        {
            throw new ArgumentException("Payment method must have a StripePaymentMethodId");
        }

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(payment.Amount.Amount * 100), // Convert to cents
            Currency = payment.Amount.Currency.ToLower(),
            Customer = tenant.StripeCustomerId,
            PaymentMethod = paymentMethod.StripePaymentMethodId,
            Confirm = true,
            OffSession = true,
            Description = payment.Description,
            Metadata = new Dictionary<string, string>
            {
                [StripeMetadataKeys.TenantId] = tenant.Id.ToString(),
                ["PaymentId"] = payment.Id.ToString()
            }
        };

        var paymentIntent = await new PaymentIntentService().CreateAsync(options);
        Logger.LogInformation("Created PaymentIntent {PaymentIntentId} for tenant {TenantId}",
            paymentIntent.Id, tenant.Id);
        return paymentIntent;
    }

    public Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId)
    {
        return new PaymentIntentService().GetAsync(paymentIntentId);
    }

    #endregion

    #region Helpers

    private static PaymentMethodBillingDetailsOptions CreateBillingDetails(PaymentMethod paymentMethod)
    {
        return new PaymentMethodBillingDetailsOptions
        {
            Name = GetPaymentMethodHolderName(paymentMethod),
            Address = paymentMethod.BillingAddress.ToStripeAddressOptions()
        };
    }

    private static string GetPaymentMethodHolderName(PaymentMethod paymentMethod)
    {
        return paymentMethod switch
        {
            CardPaymentMethod card => card.CardHolderName,
            UsBankAccountPaymentMethod usBank => usBank.AccountHolderName,
            _ => string.Empty
        };
    }

    private static PaymentMethodCardOptions MapCardPaymentMethod(CardPaymentMethod card)
    {
        return new PaymentMethodCardOptions
        {
            Number = card.CardNumber,
            ExpMonth = card.ExpMonth,
            ExpYear = card.ExpYear,
            Cvc = card.Cvc
        };
    }

    private static PaymentMethodUsBankAccountOptions MapUsBankPaymentMethod(UsBankAccountPaymentMethod usBank)
    {
        return new PaymentMethodUsBankAccountOptions
        {
            AccountNumber = usBank.AccountNumber,
            RoutingNumber = usBank.RoutingNumber,
            AccountHolderType = usBank.AccountHolderType.ToString().ToLower(),
            AccountType = usBank.AccountType.ToString().ToLower()
        };
    }

    #endregion
}
