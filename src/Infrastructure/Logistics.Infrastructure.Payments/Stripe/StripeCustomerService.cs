using Logistics.Domain.Entities;
using Microsoft.Extensions.Logging;
using Stripe;
using Logistics.Application.Abstractions.Payments.Stripe;
using StripeCustomer = Stripe.Customer;

namespace Logistics.Infrastructure.Payments.Stripe;

internal sealed class StripeCustomerService(ILogger<StripeCustomerService> logger) : IStripeCustomerService
{
    public Task<StripeCustomer> GetCustomerAsync(string stripeCustomerId)
    {
        var options = new CustomerGetOptions
        {
            Expand = ["subscriptions"]
        };
        return new CustomerService().GetAsync(stripeCustomerId, options);
    }

    public async Task<StripeCustomer> CreateCustomerAsync(Tenant tenant)
    {
        // ReSharper disable once UseObjectOrCollectionInitializer
        var options = new CustomerCreateOptions
        {
            Email = tenant.BillingEmail,
            Name = tenant.CompanyName,
            Address = tenant.CompanyAddress.ToStripeAddressOptions(),
            TaxIdData = BuildTaxIdData(tenant),
            Metadata = new Dictionary<string, string> { { StripeMetadataKeys.TenantId, tenant.Id.ToString() } }
        };

        var customer = await new CustomerService().CreateAsync(options);
        logger.LogInformation("Created Stripe customer for tenant {TenantId}", tenant.Id);
        return customer;
    }

    public Task<StripeCustomer> UpdateCustomerAsync(Tenant tenant)
    {
        if (tenant.StripeCustomerId is null)
        {
            throw new ArgumentException("Tenant must have a StripeCustomerId");
        }

        // ReSharper disable once UseObjectOrCollectionInitializer
        var options = new CustomerUpdateOptions
        {
            Email = tenant.BillingEmail,
            Name = tenant.CompanyName,
            Address = tenant.CompanyAddress.ToStripeAddressOptions(),
            Metadata = new Dictionary<string, string> { { StripeMetadataKeys.TenantId, tenant.Id.ToString() } }
        };

        return new CustomerService().UpdateAsync(tenant.StripeCustomerId, options);
    }

    /// <summary>
    /// Stripe's <c>tax_id_data</c> only exists on customer creation. Updates go through
    /// the dedicated <c>/tax_ids</c> endpoint, which we don't currently expose — VAT changes
    /// after onboarding require a manual reconciliation.
    /// </summary>
    private static List<CustomerTaxIdDataOptions>? BuildTaxIdData(Tenant tenant)
    {
        if (string.IsNullOrWhiteSpace(tenant.VatNumber))
        {
            return null;
        }

        var country = tenant.TaxResidencyCountry ?? tenant.CompanyAddress.Country;
        return
        [
            new CustomerTaxIdDataOptions
            {
                Type = StripeTaxIdTypes.Infer(country, tenant.VatNumber),
                Value = tenant.VatNumber
            }
        ];
    }

    public Task DeleteCustomerAsync(string stripeCustomerId)
    {
        var options = new CustomerDeleteOptions();
        return new CustomerService().DeleteAsync(stripeCustomerId, options);
    }
}
