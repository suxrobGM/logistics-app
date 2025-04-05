using Logistics.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using StripeCustomer = Stripe.Customer;
using StripeSubscription = Stripe.Subscription;

namespace Logistics.Application.Services;

internal class StripeService : IStripeService
{
    private readonly ILogger<StripeService> _logger;
    
    public StripeService(IOptions<StripeOptions> options, ILogger<StripeService> logger)
    {
        _logger = logger;
        StripeConfiguration.ApiKey = options.Value.SecretKey;
    }
    
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
        var options = new CustomerCreateOptions();
        options.Email = tenant.BillingEmail;
        options.Name = tenant.CompanyName;
        options.Address = new AddressOptions
        {
            Line1 = tenant.CompanyAddress.Line1,
            Line2 = tenant.CompanyAddress.Line2 ?? "",
            City = tenant.CompanyAddress.City,
            State = tenant.CompanyAddress.State,
            PostalCode = tenant.CompanyAddress.ZipCode,
            Country = tenant.CompanyAddress.Country
        };
        options.Metadata = new Dictionary<string, string> { { "tenant_id", tenant.Id } };
        
        var customer = await new CustomerService().CreateAsync(options);
        _logger.LogInformation("Created Stripe customer for tenant {TenantId}", tenant.Id);
        return customer;
    }
    
    public Task<StripeCustomer> UpdateCustomerAsync(Tenant tenant)
    {
        if (tenant.StripeCustomerId is null)
        {
            throw new ArgumentException("Tenant must have a StripeCustomerId");
        }

        // ReSharper disable once UseObjectOrCollectionInitializer
        var options = new CustomerUpdateOptions();
        options.Email = tenant.BillingEmail;
        options.Name = tenant.CompanyName;
        options.Address = new AddressOptions
        {
            Line1 = tenant.CompanyAddress.Line1,
            Line2 = tenant.CompanyAddress.Line2 ?? "",
            City = tenant.CompanyAddress.City,
            State = tenant.CompanyAddress.State,
            PostalCode = tenant.CompanyAddress.ZipCode,
            Country = tenant.CompanyAddress.Country
        };
        options.Metadata = new Dictionary<string, string> { { "tenant_id", tenant.Id } };
        
        return new CustomerService().UpdateAsync(tenant.StripeCustomerId, options);
    }

    public Task DeleteCustomerAsync(string stripeCustomerId)
    {
        var options = new CustomerDeleteOptions();
        return new CustomerService().DeleteAsync(stripeCustomerId, options);
    }

    public async Task<StripeSubscription> CreateSubscriptionAsync(SubscriptionPlan plan, Tenant tenant, int employeeCount)
    {
        if (tenant.StripeCustomerId is null)
        {
            throw new ArgumentException("Tenant must have a StripeCustomerId");
        }

        // ReSharper disable once UseObjectOrCollectionInitializer
        var options = new SubscriptionCreateOptions();
        options.Customer = tenant.StripeCustomerId;
        options.Items =
        [
            new SubscriptionItemOptions()
            {
                Price = plan.StripePriceId, // Store Stripe Price ID in SubscriptionPlan
                Quantity = employeeCount
            }
        ];
        options.Metadata = new Dictionary<string, string>
        {
            { "tenant_id", tenant.Id },
            { "plan_id", plan.Id }
        };
        options.PaymentBehavior = "default_incomplete"; // For trials or manual confirmation
        options.TrialEnd = plan.HasTrial ? DateTime.UtcNow.AddDays(30) : null;
        
        var subscription = await new SubscriptionService().CreateAsync(options);
        _logger.LogInformation("Created Stripe subscription for tenant {TenantId}", tenant.Id);
        return subscription;
    }

    public async Task CancelSubscriptionAsync(string stripeSubscriptionId, bool cancelImmediately = true)
    {
        var service = new SubscriptionService();
        
        if (cancelImmediately)
        {
            // Immediate cancellation with proration
            await service.CancelAsync(stripeSubscriptionId, new SubscriptionCancelOptions
            {
                InvoiceNow = true,
                Prorate = true
            });
            _logger.LogInformation("Canceled immediately Stripe subscription {StripeSubscriptionId}", stripeSubscriptionId);
        }
        else
        {
            // Schedule cancellation at period end
            await service.UpdateAsync(stripeSubscriptionId, new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = true
            });
            _logger.LogInformation("Canceled at period end Stripe subscription {StripeSubscriptionId}", stripeSubscriptionId);
        }
    }

    public async Task UpdateSubscriptionQuantityAsync(string stripeSubscriptionId, int employeeCount)
    {
        var subscription = await new SubscriptionService().GetAsync(stripeSubscriptionId);
        var item = subscription.Items.Data[0]; // Assuming single item per subscription
        var options = new SubscriptionItemUpdateOptions { Quantity = employeeCount };
        await new SubscriptionItemService().UpdateAsync(item.Id, options);
        _logger.LogInformation("Updated Stripe subscription {StripeSubscriptionId} with new quantity {EmployeeCount}", stripeSubscriptionId, employeeCount);
    }

    public async Task<(Product Product, Price Price)> CreateSubscriptionPlanAsync(SubscriptionPlan plan)
    {
        // 1. First create the Product
        var productService = new ProductService();
        var product = await productService.CreateAsync(new ProductCreateOptions
        {
            Name = plan.Name,
            Description = plan.Description,
            Metadata = new Dictionary<string, string>
            {
                ["plan_id"] = plan.Id
            }
        });
        
        _logger.LogInformation("Created Stripe product for plan {PlanId}", plan.Id);

        // 2. Create Price linked to the Product
        var priceService = new PriceService();
        var price = await priceService.CreateAsync(new PriceCreateOptions
        {
            Product = product.Id,
            UnitAmountDecimal = plan.Price * 100,
            Currency = plan.Currency.ToLower(),
            Recurring = new PriceRecurringOptions
            {
                Interval = "month",
                IntervalCount = 1,
                UsageType = "licensed"
            },
            Metadata = new Dictionary<string, string>
            {
                ["plan_id"] = plan.Id
            }
        });

        _logger.LogInformation("Created Stripe price for plan {PlanId}", plan.Id);
        return (product, price);
    }

    public async Task<(Product Product, Price ActivePrice)> UpdateSubscriptionPlanAsync(SubscriptionPlan plan)
    {
        if (string.IsNullOrEmpty(plan.StripeProductId))
        {
            throw new ArgumentException("SubscriptionPlan must have a StripeProductId");
        }

        if (string.IsNullOrEmpty(plan.StripePriceId))
        {
            throw new ArgumentException("SubscriptionPlan must have a StripePriceId");
        }

        // 1. Update the product metadata
        var productService = new ProductService();
        var product = await productService.UpdateAsync(plan.StripeProductId, new ProductUpdateOptions
        {
            Name = plan.Name,
            Description = plan.Description,
            Metadata = new Dictionary<string, string> 
            {
                ["plan_id"] = plan.Id
            }
        });
        
        _logger.LogInformation("Updated Stripe product for plan {PlanId}", plan.Id);

        // 2. Check if price needs update
        var priceService = new PriceService();
        var existingPrice = await priceService.GetAsync(plan.StripePriceId);
        var priceChanged = existingPrice.UnitAmountDecimal != plan.Price * 100;
        var currencyChanged = !existingPrice.Currency.Equals(plan.Currency, StringComparison.OrdinalIgnoreCase);

        var activePrice = existingPrice;
    
        // 3. Create new price if changes detected
        if (priceChanged || currencyChanged)
        {
            activePrice = await priceService.CreateAsync(new PriceCreateOptions
            {
                Product = plan.StripeProductId,
                UnitAmountDecimal = plan.Price * 100,
                Currency = plan.Currency.ToLower(),
                Recurring = new PriceRecurringOptions
                {
                    Interval = "month",
                    IntervalCount = 1,
                    UsageType = "licensed"
                },
                Metadata = new Dictionary<string, string>
                {
                    ["plan_id"] = plan.Id,
                }
            });

            // Update the plan's StripePriceId to point to the new price
            plan.StripePriceId = activePrice.Id;
            _logger.LogInformation("Created new Stripe price for plan {PlanId}", plan.Id);
        }

        return (product, activePrice);
    }
}