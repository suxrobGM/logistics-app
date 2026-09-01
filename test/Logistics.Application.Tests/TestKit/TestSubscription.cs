using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Tests.TestKit;

internal static class TestSubscription
{
    public static SubscriptionPlan CreatePlan() => new()
    {
        Name = "Starter",
        Price = new Money { Amount = 100m, Currency = "USD" },
        PerTruckPrice = new Money { Amount = 10m, Currency = "USD" },
        WeeklyAIBudgetUsd = 50m
    };

    public static Subscription Create(
        Guid tenantId,
        Guid? planId = null,
        SubscriptionStatus status = SubscriptionStatus.Active,
        string? stripeSubscriptionId = null) => new()
    {
        TenantId = tenantId,
        Tenant = TestTenant.Create(),
        PlanId = planId ?? Guid.NewGuid(),
        Plan = CreatePlan(),
        Status = status,
        StripeSubscriptionId = stripeSubscriptionId
    };
}
