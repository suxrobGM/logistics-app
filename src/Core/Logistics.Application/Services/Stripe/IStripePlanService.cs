using Logistics.Domain.Entities;
using Stripe;

namespace Logistics.Application.Services;

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
}

public record StripePlanResult(
    Product Product,
    Price BasePrice,
    Price PerTruckPrice,
    Price? AiOveragePrice = null);
