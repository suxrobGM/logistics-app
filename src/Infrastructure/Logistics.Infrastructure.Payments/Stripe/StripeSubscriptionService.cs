using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using StripeSubscription = Stripe.Subscription;
using Subscription = Logistics.Domain.Entities.Subscription;

namespace Logistics.Infrastructure.Payments.Stripe;

internal class StripeSubscriptionService(IOptions<StripeOptions> options, ILogger<StripeSubscriptionService> logger)
    : StripeServiceBase(options, logger), IStripeSubscriptionService
{
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
        Logger.LogInformation("Created Stripe subscription for tenant {TenantId}", tenant.Id);
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
            Logger.LogInformation("Canceled immediately Stripe subscription {StripeSubscriptionId}",
                stripeSubscriptionId);
            return stripeSubscription;
        }

        // Schedule cancellation at period end
        stripeSubscription = await service.UpdateAsync(stripeSubscriptionId, new SubscriptionUpdateOptions
        {
            CancelAtPeriodEnd = true
        });
        Logger.LogInformation("Canceled at period end Stripe subscription {StripeSubscriptionId}",
            stripeSubscriptionId);
        return stripeSubscription;
    }

    public async Task<SubscriptionItem> UpdateSubscriptionQuantityAsync(string stripeSubscriptionId, int truckCount)
    {
        var subscription = await new SubscriptionService().GetAsync(stripeSubscriptionId);
        var item = subscription.Items.Data.FirstOrDefault(i => i.Quantity != 1) ?? subscription.Items.Data.Last();
        var options = new SubscriptionItemUpdateOptions { Quantity = truckCount };
        var stripeSubscriptionItem = await new SubscriptionItemService().UpdateAsync(item.Id, options);
        Logger.LogInformation("Updated Stripe subscription {StripeSubscriptionId} with new truck count {TruckCount}",
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
            Logger.LogInformation("Tenant {TenantId} is creating first subscription", tenant.Id);
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

            Logger.LogInformation("Tenant {TenantId} reactivated subscription", tenant.Id);
            return upd;
        }

        // already canceled or incomplete_expired -> start fresh
        if (stripeSub.Status is "canceled" or "incomplete_expired")
        {
            Logger.LogInformation("Subscription {SubId} is canceled, creating a new one", stripeSub.Id);
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

    public async Task<StripeSubscription> ChangeSubscriptionPlanAsync(
        string stripeSubscriptionId, SubscriptionPlan newPlan, int truckCount)
    {
        var subSvc = new SubscriptionService();
        var stripeSub = await subSvc.GetAsync(stripeSubscriptionId);

        // Build items: remove existing, add new plan's prices
        var items = new List<SubscriptionItemOptions>();

        foreach (var existingItem in stripeSub.Items.Data)
        {
            items.Add(new SubscriptionItemOptions
            {
                Id = existingItem.Id,
                Deleted = true
            });
        }

        items.Add(new SubscriptionItemOptions
        {
            Price = newPlan.StripePriceId, // Base fee
            Quantity = 1
        });
        items.Add(new SubscriptionItemOptions
        {
            Price = newPlan.StripePerTruckPriceId, // Per-truck fee
            Quantity = truckCount
        });

        var updated = await subSvc.UpdateAsync(stripeSubscriptionId, new SubscriptionUpdateOptions
        {
            Items = items,
            ProrationBehavior = "create_prorations",
            Metadata = new Dictionary<string, string>
            {
                { StripeMetadataKeys.PlanId, newPlan.Id.ToString() }
            }
        });

        Logger.LogInformation(
            "Changed subscription {StripeSubscriptionId} to plan {PlanId} with {TruckCount} trucks",
            stripeSubscriptionId, newPlan.Id, truckCount);
        return updated;
    }

    #endregion

    #region Subscription Plan API

    public async Task<(Product Product, Price BasePrice, Price PerTruckPrice)> CreateSubscriptionPlanAsync(
        SubscriptionPlan plan)
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

        Logger.LogInformation("Created Stripe product for plan {PlanId}", plan.Id);

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

            Logger.LogInformation(
                "Updated Stripe prices for plan {PlanId} with billing cycle anchor {BillingCycleAnchor}", plan.Id,
                billingCycleAnchorStr);
        }

        Logger.LogInformation("Created Stripe base price and per-truck price for plan {PlanId}", plan.Id);
        return (product, basePrice, perTruckPrice);
    }

    public async Task<(Product Product, Price ActiveBasePrice, Price ActivePerTruckPrice)>
        UpdateSubscriptionPlanAsync(SubscriptionPlan plan)
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

        Logger.LogInformation("Updated Stripe product for plan {PlanId}", plan.Id);

        var priceService = new PriceService();

        // Check if base price needs update
        var existingBasePrice = await priceService.GetAsync(plan.StripePriceId);
        var basePriceChanged = existingBasePrice.UnitAmountDecimal != plan.Price * 100;
        var baseCurrencyChanged =
            !existingBasePrice.Currency.Equals(plan.Price.Currency, StringComparison.OrdinalIgnoreCase);

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
            Logger.LogInformation("Created new Stripe base price for plan {PlanId}", plan.Id);
        }

        // Check if per-truck price needs update
        Price activePerTruckPrice;

        if (!string.IsNullOrEmpty(plan.StripePerTruckPriceId))
        {
            var existingPerTruckPrice = await priceService.GetAsync(plan.StripePerTruckPriceId);
            var perTruckPriceChanged = existingPerTruckPrice.UnitAmountDecimal != plan.PerTruckPrice * 100;
            var perTruckCurrencyChanged =
                !existingPerTruckPrice.Currency.Equals(plan.PerTruckPrice.Currency,
                    StringComparison.OrdinalIgnoreCase);

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
                Logger.LogInformation("Created new Stripe per-truck price for plan {PlanId}", plan.Id);
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
            Logger.LogInformation("Created Stripe per-truck price for plan {PlanId}", plan.Id);
        }

        return (product, activeBasePrice, activePerTruckPrice);
    }

    #endregion
}
