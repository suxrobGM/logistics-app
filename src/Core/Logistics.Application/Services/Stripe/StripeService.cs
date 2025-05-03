using Logistics.Domain.Entities;
using Logistics.Domain.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using PaymentMethod = Logistics.Domain.Entities.PaymentMethod;
using StripeCustomer = Stripe.Customer;
using StripeSubscription = Stripe.Subscription;
using StripePaymentMethod = Stripe.PaymentMethod;
using Subscription = Logistics.Domain.Entities.Subscription;

namespace Logistics.Application.Services;

internal class StripeService : IStripeService
{
    private readonly ILogger<StripeService> _logger;
    
    public StripeService(IOptions<StripeOptions> options, ILogger<StripeService> logger)
    {
        _logger = logger;
        StripeConfiguration.ApiKey = options.Value.SecretKey;
    }

    #region Customer API

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
        options.Address = tenant.CompanyAddress.ToStripeAddressOptions();
        options.Metadata = new Dictionary<string, string> { { StripeMetadataKeys.TenantId, tenant.Id } };
        
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
        options.Address = tenant.CompanyAddress.ToStripeAddressOptions();
        options.Metadata = new Dictionary<string, string> { { StripeMetadataKeys.TenantId, tenant.Id } };
        
        return new CustomerService().UpdateAsync(tenant.StripeCustomerId, options);
    }

    public Task DeleteCustomerAsync(string stripeCustomerId)
    {
        var options = new CustomerDeleteOptions();
        return new CustomerService().DeleteAsync(stripeCustomerId, options);
    }

    #endregion
    
    #region Subscription API

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
            { StripeMetadataKeys.TenantId, tenant.Id },
            { StripeMetadataKeys.PlanId, plan.Id }
        };
        options.PaymentBehavior = "default_incomplete"; // For trials or manual confirmation
        options.BillingCycleAnchor = plan.BillingCycleAnchor;
        options.TrialEnd = SubscriptionUtils.GetTrialEndDate(plan.TrialPeriod);
        
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
    
    public async Task<StripeSubscription?> RenewSubscriptionAsync(
        Subscription? subEntity,
        SubscriptionPlan plan,
        Tenant tenant,
        int employeeCount)
    {
        if (string.IsNullOrEmpty(tenant.StripeCustomerId))
            throw new ArgumentException("Tenant must have a StripeCustomerId");

        var subSvc  = new SubscriptionService();
        var invSvc  = new InvoiceService();
        
        // Never had a Stripe subscription → create new one
        if (subEntity is null || string.IsNullOrEmpty(subEntity.StripeSubscriptionId))
        {
            _logger.LogInformation("Tenant {TenantId} is creating first subscription", tenant.Id);
            return await CreateSubscriptionAsync(plan, tenant, employeeCount);
        }

        // Pull the live object so we know the exact Stripe state
        var stripeSub = await subSvc.GetAsync(subEntity.StripeSubscriptionId);
        
        // Had subscription, but it was set to cancel at period end → re‑activate
        if (stripeSub.CancelAtPeriodEnd ||
            stripeSub.Status is "canceled" or "incomplete_expired")
        {
            _logger.LogInformation("Re‑activating Stripe subscription {SubscriptionId}", stripeSub.Id);

            var updOpts = new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = false,
                BillingCycleAnchor = SubscriptionBillingCycleAnchor.Now,
                ProrationBehavior = "none",
                Items =
                [
                    new SubscriptionItemOptions
                    {
                        Id = stripeSub.Items.Data[0].Id,
                        Price = plan.StripePriceId,
                        Quantity = employeeCount
                    }
                ]
            };
            updOpts.AddExpand("latest_invoice.payment_intent");

            stripeSub = await subSvc.UpdateAsync(stripeSub.Id, updOpts);
            return stripeSub;
        }
        
        // Subscription already active → charge now by issuing an invoice
        _logger.LogInformation("Tenant {TenantId} already active – issuing invoice", tenant.Id);

        var invoice = await invSvc.CreateAsync(new InvoiceCreateOptions
        {
            Customer      = tenant.StripeCustomerId,
            Subscription  = stripeSub.Id,
            AutoAdvance   = true // finalises automatically
        });

        // finalise immediately if AutoAdvance was false
        if (invoice.Status == "draft")
        {
            await invSvc.FinalizeInvoiceAsync(invoice.Id);
        }

        // nothing new to return – but method stays symmetrical
        return stripeSub;
    }

    #endregion

    #region Subscription Plan API

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
                [StripeMetadataKeys.PlanId] = plan.Id
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
                Interval = plan.Interval.ToString().ToLower(),
                IntervalCount = plan.IntervalCount,
                UsageType = "licensed"
            },
            Metadata = new Dictionary<string, string>
            {
                [StripeMetadataKeys.PlanId] = plan.Id
            }
        });
        
        if (plan.BillingCycleAnchor.HasValue)
        {
            var billingCycleAnchorStr = plan.BillingCycleAnchor.Value.ToString("O");
            await new PriceService().UpdateAsync(price.Id, new PriceUpdateOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    [StripeMetadataKeys.BillingCycleAnchor] = billingCycleAnchorStr
                }
            });
            
            _logger.LogInformation("Updated Stripe price for plan {PlanId} with billing cycle anchor {BillingCycleAnchor}", plan.Id, billingCycleAnchorStr);
        }

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
                [StripeMetadataKeys.PlanId] = plan.Id
            }
        });
        
        _logger.LogInformation("Updated Stripe product for plan {PlanId}", plan.Id);

        // Check if price needs update
        var priceService = new PriceService();
        var existingPrice = await priceService.GetAsync(plan.StripePriceId);
        var priceChanged = existingPrice.UnitAmountDecimal != plan.Price * 100;
        var currencyChanged = !existingPrice.Currency.Equals(plan.Currency, StringComparison.OrdinalIgnoreCase);
        
        // Check if billing cycle changed
        var billingChanged =
            !existingPrice.Recurring.Interval.Equals(plan.Interval.ToString(),
                StringComparison.CurrentCultureIgnoreCase) ||
            existingPrice.Recurring.IntervalCount != plan.IntervalCount;

        var activePrice = existingPrice;
        
        // Create a new price if any of the price, currency, or billing cycle has changed
        if (priceChanged || currencyChanged || billingChanged)
        {
            activePrice = await priceService.CreateAsync(new PriceCreateOptions
            {
                Product = plan.StripeProductId,
                UnitAmountDecimal = plan.Price * 100,
                Currency = plan.Currency.ToLower(),
                Recurring = new PriceRecurringOptions
                {
                    Interval = plan.Interval.ToString().ToLower(),
                    IntervalCount = plan.IntervalCount,
                    UsageType = "licensed"
                },
                Metadata = new Dictionary<string, string>
                {
                    [StripeMetadataKeys.PlanId] = plan.Id,
                }
            });

            // Update the plan's StripePriceId to point to the new price
            plan.StripePriceId = activePrice.Id;
            _logger.LogInformation("Created new Stripe price for plan {PlanId}", plan.Id);
        }

        return (product, activePrice);
    }

    #endregion

    #region Payment Method API

    public async Task<StripePaymentMethod> UpdatePaymentMethodAsync(PaymentMethod paymentMethod)
    {
        if (string.IsNullOrEmpty(paymentMethod.StripePaymentMethodId))
            throw new ArgumentException("Payment method must have a Stripe ID");

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
        _logger.LogInformation("Updated Stripe payment method {PaymentMethodId}", paymentMethod.StripePaymentMethodId);
        return updatedMethod;
    }

    public async Task RemovePaymentMethodAsync(PaymentMethod paymentMethod)
    {
        if (string.IsNullOrEmpty(paymentMethod.StripePaymentMethodId))
            throw new ArgumentException("Payment method must have a Stripe ID");

        await new PaymentMethodService().DetachAsync(paymentMethod.StripePaymentMethodId);
        _logger.LogInformation("Removed Stripe payment method {PaymentMethodId}", paymentMethod.StripePaymentMethodId);
    }

    public async Task SetDefaultPaymentMethodAsync(PaymentMethod paymentMethod, Tenant tenant)
    {
        if (string.IsNullOrEmpty(tenant.StripeCustomerId))
            throw new ArgumentException("Tenant must have a StripeCustomerId");

        if (string.IsNullOrEmpty(paymentMethod.StripePaymentMethodId))
            throw new ArgumentException("Payment method must have a StripePaymentMethodId");

        await new CustomerService().UpdateAsync(tenant.StripeCustomerId, new CustomerUpdateOptions
        {
            InvoiceSettings = new CustomerInvoiceSettingsOptions 
            { 
                DefaultPaymentMethod = paymentMethod.StripePaymentMethodId 
            }
        });
        
        _logger.LogInformation("Set default Stripe payment method for tenant {TenantId}, Stripe payment method ID {StripePaymentMethodId}", 
            tenant.Id, paymentMethod.StripePaymentMethodId);
    }

    #endregion

    #region Setup Intent API

    public async Task<SetupIntent> CreateSetupIntentAsync(Tenant tenant)
    {
        if (string.IsNullOrEmpty(tenant.StripeCustomerId))
            throw new ArgumentException("Tenant must have a StripeCustomerId");

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
                [StripeMetadataKeys.TenantId] = tenant.Id
            }
        };

        var setupIntentService = new SetupIntentService();
        var setupIntent = await setupIntentService.CreateAsync(options);
        _logger.LogInformation("Created SetupIntent for tenant {TenantId}", tenant.Id);
        return setupIntent;
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