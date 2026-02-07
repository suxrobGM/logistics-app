using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.DbMigrator.Seeders.Infrastructure;

/// <summary>
/// Seeds subscription plans with upsert logic.
/// </summary>
internal class SubscriptionPlanSeeder(ILogger<SubscriptionPlanSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(SubscriptionPlanSeeder);
    public override SeederType Type => SeederType.Infrastructure;
    public override int Order => 30;

    private static readonly TenantFeature[] starterFeatures =
    [
        TenantFeature.Dashboard,
        TenantFeature.Employees,
        TenantFeature.Loads,
        TenantFeature.Trucks,
        TenantFeature.Customers,
        TenantFeature.Invoices,
        TenantFeature.Payments,
        TenantFeature.Trips,
        TenantFeature.Messages,
        TenantFeature.Notifications,
        TenantFeature.Expenses,
        TenantFeature.Reports
    ];

    private static readonly TenantFeature[] professionalFeatures =
    [
        ..starterFeatures,
        TenantFeature.Eld,
        TenantFeature.LoadBoard,
        TenantFeature.Payroll,
        TenantFeature.Timesheets
    ];

    private static readonly TenantFeature[] enterpriseFeatures =
        Enum.GetValues<TenantFeature>();

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();
        var planRepo = context.MasterUnitOfWork.Repository<SubscriptionPlan>();
        var featureRepo = context.MasterUnitOfWork.Repository<PlanFeature>();

        var plans = new[]
        {
            new
            {
                Name = "Starter",
                Description = "Essential tools for small fleets getting started",
                Price = 19m,
                PerTruckPrice = 12m,
                MaxTrucks = (int?)10,
                AnnualDiscountPercent = 15m,
                Tier = PlanTier.Starter,
                TrialPeriod = TrialPeriod.ThirtyDays,
                Features = starterFeatures
            },
            new
            {
                Name = "Professional",
                Description = "Advanced features for growing fleets",
                Price = 79m,
                PerTruckPrice = 7m,
                MaxTrucks = (int?)50,
                AnnualDiscountPercent = 20m,
                Tier = PlanTier.Professional,
                TrialPeriod = TrialPeriod.ThirtyDays,
                Features = professionalFeatures
            },
            new
            {
                Name = "Enterprise",
                Description = "Full platform access for large operations",
                Price = 149m,
                PerTruckPrice = 4m,
                MaxTrucks = (int?)null,
                AnnualDiscountPercent = 20m,
                Tier = PlanTier.Enterprise,
                TrialPeriod = TrialPeriod.ThirtyDays,
                Features = enterpriseFeatures
            }
        };

        foreach (var planDef in plans)
        {
            var existingPlan = await planRepo.GetAsync(i => i.Name == planDef.Name, cancellationToken);

            if (existingPlan is null)
            {
                var newPlan = new SubscriptionPlan
                {
                    Name = planDef.Name,
                    Description = planDef.Description,
                    Price = planDef.Price,
                    PerTruckPrice = planDef.PerTruckPrice,
                    MaxTrucks = planDef.MaxTrucks,
                    AnnualDiscountPercent = planDef.AnnualDiscountPercent,
                    Tier = planDef.Tier,
                    TrialPeriod = planDef.TrialPeriod
                };

                await planRepo.AddAsync(newPlan, cancellationToken);
                await context.MasterUnitOfWork.SaveChangesAsync(cancellationToken);

                foreach (var feature in planDef.Features)
                {
                    await featureRepo.AddAsync(new PlanFeature
                    {
                        PlanId = newPlan.Id,
                        Feature = feature
                    }, cancellationToken);
                }

                await context.MasterUnitOfWork.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Created subscription plan '{PlanName}' with {FeatureCount} features",
                    planDef.Name, planDef.Features.Length);
            }
            else
            {
                // Upsert plan properties
                var updated = false;

                if (existingPlan.Description != planDef.Description)
                {
                    existingPlan.Description = planDef.Description;
                    updated = true;
                }

                if (existingPlan.Price != planDef.Price)
                {
                    existingPlan.Price = planDef.Price;
                    updated = true;
                }

                if (existingPlan.PerTruckPrice != planDef.PerTruckPrice)
                {
                    existingPlan.PerTruckPrice = planDef.PerTruckPrice;
                    updated = true;
                }

                if (existingPlan.MaxTrucks != planDef.MaxTrucks)
                {
                    existingPlan.MaxTrucks = planDef.MaxTrucks;
                    updated = true;
                }

                if (existingPlan.AnnualDiscountPercent != planDef.AnnualDiscountPercent)
                {
                    existingPlan.AnnualDiscountPercent = planDef.AnnualDiscountPercent;
                    updated = true;
                }

                if (existingPlan.Tier != planDef.Tier)
                {
                    existingPlan.Tier = planDef.Tier;
                    updated = true;
                }

                if (existingPlan.TrialPeriod != planDef.TrialPeriod)
                {
                    existingPlan.TrialPeriod = planDef.TrialPeriod;
                    updated = true;
                }

                // Upsert features: remove old ones and recreate
                var existingFeatures = await featureRepo.GetListAsync(
                    f => f.PlanId == existingPlan.Id, cancellationToken);

                var existingFeatureSet = existingFeatures.Select(f => f.Feature).ToHashSet();
                var desiredFeatureSet = planDef.Features.ToHashSet();

                if (!existingFeatureSet.SetEquals(desiredFeatureSet))
                {
                    foreach (var oldFeature in existingFeatures)
                    {
                        featureRepo.Delete(oldFeature);
                    }

                    foreach (var feature in planDef.Features)
                    {
                        await featureRepo.AddAsync(new PlanFeature
                        {
                            PlanId = existingPlan.Id,
                            Feature = feature
                        }, cancellationToken);
                    }

                    updated = true;
                }

                if (updated)
                {
                    await context.MasterUnitOfWork.SaveChangesAsync(cancellationToken);
                    logger.LogInformation("Updated subscription plan '{PlanName}'", planDef.Name);
                }
                else
                {
                    logger.LogInformation("Subscription plan '{PlanName}' already up to date", planDef.Name);
                }
            }
        }

        LogCompleted();
    }
}
