using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using StripeCustomer = Stripe.Customer;

namespace Logistics.Infrastructure.Payments.Stripe;

internal class StripeCustomerService(IOptions<StripeOptions> options, ILogger<StripeCustomerService> logger)
    : StripeServiceBase(options, logger), IStripeCustomerService
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
            Address = tenant.CompanyAddress?.ToStripeAddressOptions(),
            Metadata = new Dictionary<string, string> { { StripeMetadataKeys.TenantId, tenant.Id.ToString() } }
        };

        var customer = await new CustomerService().CreateAsync(options);
        Logger.LogInformation("Created Stripe customer for tenant {TenantId}", tenant.Id);
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
            Address = tenant.CompanyAddress?.ToStripeAddressOptions(),
            Metadata = new Dictionary<string, string> { { StripeMetadataKeys.TenantId, tenant.Id.ToString() } }
        };

        return new CustomerService().UpdateAsync(tenant.StripeCustomerId, options);
    }

    public Task DeleteCustomerAsync(string stripeCustomerId)
    {
        var options = new CustomerDeleteOptions();
        return new CustomerService().DeleteAsync(stripeCustomerId, options);
    }
}
