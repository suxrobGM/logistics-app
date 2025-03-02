using Logistics.Domain.Entities;
using Microsoft.Extensions.Options;
using Stripe;
using StripeCustomer = Stripe.Customer;
using StripeSubscription = Stripe.Subscription;

namespace Logistics.Application.Services;

internal class StripeService : IStripeService
{
    public StripeService(IOptions<StripeOptions> options)
    {
        StripeConfiguration.ApiKey = options.Value.SecretKey;
    }
    
    public Task<StripeCustomer> CreateCustomerAsync(Tenant tenant)
    {
        var options = new CustomerCreateOptions
        {
            Email = tenant.BillingEmail,
            Name = tenant.CompanyName,
            Address =
            {
                Line1 = tenant.CompanyAddress.Line1,
                Line2 = tenant.CompanyAddress.Line2,
                City = tenant.CompanyAddress.City,
                State = tenant.CompanyAddress.State,
                PostalCode = tenant.CompanyAddress.ZipCode,
                Country = tenant.CompanyAddress.Country
            },
            Metadata = new Dictionary<string, string> { { "TenantId", tenant.Id } }
        };
        return new CustomerService().CreateAsync(options);
    }

    public Task<StripeSubscription> CreateSubscriptionAsync(SubscriptionPlan plan, string stripeCustomerId, int employeeCount)
    {
        var options = new SubscriptionCreateOptions
        {
            Customer = stripeCustomerId,
            Items =
            [
                new SubscriptionItemOptions
                {
                    Price = plan.StripePriceId, // Store Stripe Price ID in SubscriptionPlan
                    Quantity = employeeCount
                }
            ],
            PaymentBehavior = "default_incomplete", // For trials or manual confirmation
            TrialEnd = plan.HasTrial ? DateTime.UtcNow.AddDays(30) : null
        };
        return new SubscriptionService().CreateAsync(options);
    }

    public async Task UpdateSubscriptionQuantityAsync(string stripeSubscriptionId, int employeeCount)
    {
        var subscription = await new SubscriptionService().GetAsync(stripeSubscriptionId);
        var item = subscription.Items.Data[0]; // Assuming single item per subscription
        var options = new SubscriptionItemUpdateOptions { Quantity = employeeCount };
        await new SubscriptionItemService().UpdateAsync(item.Id, options);
    }
}