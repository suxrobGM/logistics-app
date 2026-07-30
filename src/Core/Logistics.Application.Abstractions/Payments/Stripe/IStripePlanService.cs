using Logistics.Domain.Entities;
using Stripe;
using Logistics.Application.Abstractions.Payments.Stripe;

namespace Logistics.Application.Abstractions.Payments.Stripe;

/// <summary>
/// Manages subscription plan products and prices in Stripe.
/// </summary>
public interface IStripePlanService
{
    /// <summary>
    /// Creates a Stripe product with base, per-truck, and optional AI overage prices for a plan.
    /// </summary>
    Task<StripePlanResult> CreatePlanAsync(SubscriptionPlan plan);

    /// <summary>
    /// Updates an existing plan's Stripe product and recreates prices if amounts/currency/billing changed.
    /// </summary>
    Task<StripePlanResult> UpdatePlanAsync(SubscriptionPlan plan);

    /// <summary>
    /// Creates or updates the plan's Stripe resources, whichever it needs, and writes the resulting
    /// ids onto <paramref name="plan"/>. Stored ids that no longer resolve in Stripe - a deleted
    /// product, or a database pointed at a different Stripe account - are treated as unsynced and
    /// rebuilt rather than throwing. The caller still owns saving the plan.
    /// </summary>
    Task<StripePlanResult> SyncPlanAsync(SubscriptionPlan plan);
}

public record StripePlanResult(
    Product Product,
    Price BasePrice,
    Price PerTruckPrice,
    Price? AIOveragePrice = null);
