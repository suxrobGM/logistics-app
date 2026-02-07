using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using PaymentMethod = Logistics.Domain.Entities.PaymentMethod;
using StripeCustomer = Stripe.Customer;
using StripePaymentMethod = Stripe.PaymentMethod;
using StripeSubscription = Stripe.Subscription;
using Subscription = Logistics.Domain.Entities.Subscription;

namespace Logistics.Infrastructure.Payments.Stripe;

internal class StripeService : IStripeService
{
    private readonly ILogger<StripeService> logger;

    public StripeService(IOptions<StripeOptions> options, ILogger<StripeService> logger)
    {
        this.logger = logger;
        StripeConfiguration.ApiKey = options.Value.SecretKey;
        WebhookSecret = options.Value.WebhookSecret ??
                        throw new ArgumentNullException(nameof(options.Value.WebhookSecret));
    }

    public string WebhookSecret { get; }

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
        logger.LogInformation("Created SetupIntent for tenant {TenantId}", tenant.Id);
        return setupIntent;
    }

    #endregion

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
        var options = new CustomerCreateOptions
        {
            Email = tenant.BillingEmail,
            Name = tenant.CompanyName,
            Address = tenant.CompanyAddress?.ToStripeAddressOptions(),
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

    #endregion

    #region Subscription API

    public async Task<StripeSubscription> CreateSubscriptionAsync(SubscriptionPlan plan, Tenant tenant,
        int truckCount, bool trial = false)
    {
        if (tenant.StripeCustomerId is null)
        {
            throw new ArgumentException("Tenant must have a StripeCustomerId");
        }

        // ReSharper disable once UseObjectOrCollectionInitializer
        var options = new SubscriptionCreateOptions
        {
            Customer = tenant.StripeCustomerId,
            Items =
            [
                new SubscriptionItemOptions
                {
                    Price = plan.StripePriceId, // Base monthly fee
                    Quantity = 1
                },
                new SubscriptionItemOptions
                {
                    Price = plan.StripePerTruckPriceId, // Per-truck fee
                    Quantity = truckCount
                }
            ],
            Metadata = new Dictionary<string, string>
            {
                { StripeMetadataKeys.TenantId, tenant.Id.ToString() },
                { StripeMetadataKeys.PlanId, plan.Id.ToString() }
            },

            BillingCycleAnchor = plan.BillingCycleAnchor
        };

        if (trial)
        {
            options.PaymentBehavior = "default_incomplete"; // For trials or manual confirmation
            options.TrialEnd = SubscriptionUtils.GetTrialEndDate(plan.TrialPeriod);
        }

        var subscription = await new SubscriptionService().CreateAsync(options);
        logger.LogInformation("Created Stripe subscription for tenant {TenantId}", tenant.Id);
        return subscription;
    }

    public async Task<StripeSubscription> CancelSubscriptionAsync(string stripeSubscriptionId,
        bool cancelImmediately = true)
    {
        var service = new SubscriptionService();
        StripeSubscription stripeSubscription;

        if (cancelImmediately)
        {
            // Immediate cancellation with proration
            stripeSubscription = await service.CancelAsync(stripeSubscriptionId, new SubscriptionCancelOptions
            {
                InvoiceNow = true,
                Prorate = true
            });
            logger.LogInformation("Canceled immediately Stripe subscription {StripeSubscriptionId}",
                stripeSubscriptionId);
            return stripeSubscription;
        }

        // Schedule cancellation at period end
        stripeSubscription = await service.UpdateAsync(stripeSubscriptionId, new SubscriptionUpdateOptions
        {
            CancelAtPeriodEnd = true
        });
        logger.LogInformation("Canceled at period end Stripe subscription {StripeSubscriptionId}",
            stripeSubscriptionId);
        return stripeSubscription;
    }

    public async Task<SubscriptionItem> UpdateSubscriptionQuantityAsync(string stripeSubscriptionId, int truckCount)
    {
        var subscription = await new SubscriptionService().GetAsync(stripeSubscriptionId);
        var item = subscription.Items.Data.FirstOrDefault(i => i.Quantity != 1) ?? subscription.Items.Data.Last();
        var options = new SubscriptionItemUpdateOptions { Quantity = truckCount };
        var stripeSubscriptionItem = await new SubscriptionItemService().UpdateAsync(item.Id, options);
        logger.LogInformation("Updated Stripe subscription {StripeSubscriptionId} with new truck count {TruckCount}",
            stripeSubscriptionId, truckCount);
        return stripeSubscriptionItem;
    }

    public async Task<StripeSubscription> RenewSubscriptionAsync(
        Subscription? subEntity,
        SubscriptionPlan plan,
        Tenant tenant,
        int truckCount)
    {
        if (string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            throw new ArgumentException("Tenant must have a StripeCustomerId");
        }

        var subSvc = new SubscriptionService();
        var invSvc = new InvoiceService();

        // Never had a Stripe subscription → create new one
        if (subEntity is null || string.IsNullOrEmpty(subEntity.StripeSubscriptionId))
        {
            logger.LogInformation("Tenant {TenantId} is creating first subscription", tenant.Id);
            return await CreateSubscriptionAsync(plan, tenant, truckCount);
        }

        var stripeSub = await subSvc.GetAsync(subEntity.StripeSubscriptionId);

        // Scheduled to cancel but NOT canceled yet -> reactivate
        if (stripeSub.CancelAtPeriodEnd && stripeSub.Status != "canceled")
        {
            var upd = await subSvc.UpdateAsync(stripeSub.Id, new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = false,
                BillingCycleAnchor = SubscriptionBillingCycleAnchor.Now,
                ProrationBehavior = "none"
            });

            logger.LogInformation("Tenant {TenantId} reactivated subscription", tenant.Id);
            return upd;
        }

        // already canceled or incomplete_expired -> start fresh
        if (stripeSub.Status is "canceled" or "incomplete_expired")
        {
            logger.LogInformation("Subscription {SubId} is canceled, creating a new one", stripeSub.Id);
            return await CreateSubscriptionAsync(plan, tenant, truckCount);
        }

        // still fully active -> just bill immediately
        var invoice = await invSvc.CreateAsync(new InvoiceCreateOptions
        {
            Customer = tenant.StripeCustomerId,
            Subscription = stripeSub.Id,
            AutoAdvance = true
        });

        if (invoice.Status == "draft")
        {
            await invSvc.FinalizeInvoiceAsync(invoice.Id);
        }

        return stripeSub;
    }

    #endregion

    #region Subscription Plan API

    public async Task<(Product Product, Price BasePrice, Price PerTruckPrice)> CreateSubscriptionPlanAsync(SubscriptionPlan plan)
    {
        // 1. First create the Product
        var productService = new ProductService();
        var product = await productService.CreateAsync(new ProductCreateOptions
        {
            Name = plan.Name,
            Description = plan.Description,
            Metadata = new Dictionary<string, string>
            {
                [StripeMetadataKeys.PlanId] = plan.Id.ToString()
            }
        });

        logger.LogInformation("Created Stripe product for plan {PlanId}", plan.Id);

        // 2. Create base price linked to the Product
        var priceService = new PriceService();
        var basePrice = await priceService.CreateAsync(new PriceCreateOptions
        {
            Product = product.Id,
            UnitAmountDecimal = plan.Price * 100,
            Currency = plan.Price.Currency.ToLower(),
            Recurring = new PriceRecurringOptions
            {
                Interval = plan.Interval.ToString().ToLower(),
                IntervalCount = plan.IntervalCount,
                UsageType = "licensed"
            },
            Metadata = new Dictionary<string, string>
            {
                [StripeMetadataKeys.PlanId] = plan.Id.ToString(),
                ["price_type"] = "base"
            }
        });

        // 3. Create per-truck price linked to the Product
        var perTruckPrice = await priceService.CreateAsync(new PriceCreateOptions
        {
            Product = product.Id,
            UnitAmountDecimal = plan.PerTruckPrice * 100,
            Currency = plan.PerTruckPrice.Currency.ToLower(),
            Recurring = new PriceRecurringOptions
            {
                Interval = plan.Interval.ToString().ToLower(),
                IntervalCount = plan.IntervalCount,
                UsageType = "licensed"
            },
            Metadata = new Dictionary<string, string>
            {
                [StripeMetadataKeys.PlanId] = plan.Id.ToString(),
                ["price_type"] = "per_truck"
            }
        });

        if (plan.BillingCycleAnchor.HasValue)
        {
            var billingCycleAnchorStr = plan.BillingCycleAnchor.Value.ToString("O");
            await priceService.UpdateAsync(basePrice.Id, new PriceUpdateOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    [StripeMetadataKeys.BillingCycleAnchor] = billingCycleAnchorStr,
                    ["price_type"] = "base"
                }
            });
            await priceService.UpdateAsync(perTruckPrice.Id, new PriceUpdateOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    [StripeMetadataKeys.BillingCycleAnchor] = billingCycleAnchorStr,
                    ["price_type"] = "per_truck"
                }
            });

            logger.LogInformation(
                "Updated Stripe prices for plan {PlanId} with billing cycle anchor {BillingCycleAnchor}", plan.Id,
                billingCycleAnchorStr);
        }

        logger.LogInformation("Created Stripe base price and per-truck price for plan {PlanId}", plan.Id);
        return (product, basePrice, perTruckPrice);
    }

    public async Task<(Product Product, Price ActiveBasePrice, Price ActivePerTruckPrice)> UpdateSubscriptionPlanAsync(SubscriptionPlan plan)
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
                [StripeMetadataKeys.PlanId] = plan.Id.ToString()
            }
        });

        logger.LogInformation("Updated Stripe product for plan {PlanId}", plan.Id);

        var priceService = new PriceService();

        // Check if base price needs update
        var existingBasePrice = await priceService.GetAsync(plan.StripePriceId);
        var basePriceChanged = existingBasePrice.UnitAmountDecimal != plan.Price * 100;
        var baseCurrencyChanged = !existingBasePrice.Currency.Equals(plan.Price.Currency, StringComparison.OrdinalIgnoreCase);

        // Check if billing cycle changed
        var billingChanged =
            !existingBasePrice.Recurring.Interval.Equals(plan.Interval.ToString(),
                StringComparison.CurrentCultureIgnoreCase) ||
            existingBasePrice.Recurring.IntervalCount != plan.IntervalCount;

        var activeBasePrice = existingBasePrice;

        // Create a new base price if any of the price, currency, or billing cycle has changed
        if (basePriceChanged || baseCurrencyChanged || billingChanged)
        {
            activeBasePrice = await priceService.CreateAsync(new PriceCreateOptions
            {
                Product = plan.StripeProductId,
                UnitAmountDecimal = plan.Price * 100,
                Currency = plan.Price.Currency.ToLower(),
                Recurring = new PriceRecurringOptions
                {
                    Interval = plan.Interval.ToString().ToLower(),
                    IntervalCount = plan.IntervalCount,
                    UsageType = "licensed"
                },
                Metadata = new Dictionary<string, string>
                {
                    [StripeMetadataKeys.PlanId] = plan.Id.ToString(),
                    ["price_type"] = "base"
                }
            });

            plan.StripePriceId = activeBasePrice.Id;
            logger.LogInformation("Created new Stripe base price for plan {PlanId}", plan.Id);
        }

        // Check if per-truck price needs update
        Price activePerTruckPrice;

        if (!string.IsNullOrEmpty(plan.StripePerTruckPriceId))
        {
            var existingPerTruckPrice = await priceService.GetAsync(plan.StripePerTruckPriceId);
            var perTruckPriceChanged = existingPerTruckPrice.UnitAmountDecimal != plan.PerTruckPrice * 100;
            var perTruckCurrencyChanged = !existingPerTruckPrice.Currency.Equals(plan.PerTruckPrice.Currency, StringComparison.OrdinalIgnoreCase);

            activePerTruckPrice = existingPerTruckPrice;

            if (perTruckPriceChanged || perTruckCurrencyChanged || billingChanged)
            {
                activePerTruckPrice = await priceService.CreateAsync(new PriceCreateOptions
                {
                    Product = plan.StripeProductId,
                    UnitAmountDecimal = plan.PerTruckPrice * 100,
                    Currency = plan.PerTruckPrice.Currency.ToLower(),
                    Recurring = new PriceRecurringOptions
                    {
                        Interval = plan.Interval.ToString().ToLower(),
                        IntervalCount = plan.IntervalCount,
                        UsageType = "licensed"
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        [StripeMetadataKeys.PlanId] = plan.Id.ToString(),
                        ["price_type"] = "per_truck"
                    }
                });

                plan.StripePerTruckPriceId = activePerTruckPrice.Id;
                logger.LogInformation("Created new Stripe per-truck price for plan {PlanId}", plan.Id);
            }
        }
        else
        {
            // No existing per-truck price, create one
            activePerTruckPrice = await priceService.CreateAsync(new PriceCreateOptions
            {
                Product = plan.StripeProductId,
                UnitAmountDecimal = plan.PerTruckPrice * 100,
                Currency = plan.PerTruckPrice.Currency.ToLower(),
                Recurring = new PriceRecurringOptions
                {
                    Interval = plan.Interval.ToString().ToLower(),
                    IntervalCount = plan.IntervalCount,
                    UsageType = "licensed"
                },
                Metadata = new Dictionary<string, string>
                {
                    [StripeMetadataKeys.PlanId] = plan.Id.ToString(),
                    ["price_type"] = "per_truck"
                }
            });

            plan.StripePerTruckPriceId = activePerTruckPrice.Id;
            logger.LogInformation("Created Stripe per-truck price for plan {PlanId}", plan.Id);
        }

        return (product, activeBasePrice, activePerTruckPrice);
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
        logger.LogInformation("Updated Stripe payment method {PaymentMethodId}", paymentMethod.StripePaymentMethodId);
        return updatedMethod;
    }

    public async Task RemovePaymentMethodAsync(PaymentMethod paymentMethod)
    {
        if (string.IsNullOrEmpty(paymentMethod.StripePaymentMethodId))
        {
            throw new ArgumentException("Payment method must have a Stripe ID");
        }

        await new PaymentMethodService().DetachAsync(paymentMethod.StripePaymentMethodId);
        logger.LogInformation("Removed Stripe payment method {PaymentMethodId}", paymentMethod.StripePaymentMethodId);
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

        logger.LogInformation(
            "Set default Stripe payment method for tenant {TenantId}, Stripe payment method ID {StripePaymentMethodId}",
            tenant.Id, paymentMethod.StripePaymentMethodId);
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
        logger.LogInformation("Created PaymentIntent {PaymentIntentId} for tenant {TenantId}",
            paymentIntent.Id, tenant.Id);
        return paymentIntent;
    }

    public Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId)
    {
        return new PaymentIntentService().GetAsync(paymentIntentId);
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

        logger.LogInformation("Attached payment method {PaymentMethodId} to customer {CustomerId}",
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
