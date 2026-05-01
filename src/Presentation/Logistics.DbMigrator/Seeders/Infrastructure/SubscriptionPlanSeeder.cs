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

    private static readonly TenantFeature[] StarterFeatures =
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
        TenantFeature.Reports,
        TenantFeature.Eld,
        TenantFeature.AgenticDispatch
    ];

    private static readonly TenantFeature[] ProfessionalFeatures =
    [
        ..StarterFeatures,
        TenantFeature.Eld,
        TenantFeature.LoadBoard,
        TenantFeature.Payroll,
        TenantFeature.Timesheets,
        TenantFeature.Safety,
        TenantFeature.Maintenance,
        TenantFeature.TelegramBot,
        TenantFeature.McpServer
    ];

    private static readonly TenantFeature[] EnterpriseFeatures = Enum.GetValues<TenantFeature>();

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
                Price = 29m,
                PerTruckPrice = 12m,
                MaxTrucks = (int?)10,
                Tier = PlanTier.Starter,
                WeeklyAiRequestQuota = (int?)100,
                AllowedModelTier = LlmModelTier.Base,
                Features = StarterFeatures
            },
            new
            {
                Name = "Professional",
                Description = "Advanced features for growing fleets",
                Price = 79m,
                PerTruckPrice = 9m,
                MaxTrucks = (int?)30,
                Tier = PlanTier.Professional,
                WeeklyAiRequestQuota = (int?)400,
                AllowedModelTier = LlmModelTier.Premium,
                Features = ProfessionalFeatures
            },
            new
            {
                Name = "Enterprise",
                Description = "Full platform access for large operations",
                Price = 169m,
                PerTruckPrice = 6m,
                MaxTrucks = (int?)null,
                Tier = PlanTier.Enterprise,
                WeeklyAiRequestQuota = (int?)800,
                AllowedModelTier = LlmModelTier.Ultra,
                Features = EnterpriseFeatures
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
                    Price = new() { Amount = planDef.Price, Currency = "USD" },
                    PerTruckPrice = new() { Amount = planDef.PerTruckPrice, Currency = "USD" },
                    MaxTrucks = planDef.MaxTrucks,
                    Tier = planDef.Tier,
                    WeeklyAiRequestQuota = planDef.WeeklyAiRequestQuota,
                    AllowedModelTier = planDef.AllowedModelTier
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

                if (existingPlan.Price.Amount != planDef.Price)
                {
                    existingPlan.Price = new() { Amount = planDef.Price, Currency = existingPlan.Price.Currency };
                    updated = true;
                }

                if (existingPlan.PerTruckPrice.Amount != planDef.PerTruckPrice)
                {
                    existingPlan.PerTruckPrice = new() { Amount = planDef.PerTruckPrice, Currency = existingPlan.PerTruckPrice.Currency };
                    updated = true;
                }

                if (existingPlan.MaxTrucks != planDef.MaxTrucks)
                {
                    existingPlan.MaxTrucks = planDef.MaxTrucks;
                    updated = true;
                }

                if (existingPlan.Tier != planDef.Tier)
                {
                    existingPlan.Tier = planDef.Tier;
                    updated = true;
                }

                if (existingPlan.WeeklyAiRequestQuota != planDef.WeeklyAiRequestQuota)
                {
                    existingPlan.WeeklyAiRequestQuota = planDef.WeeklyAiRequestQuota;
                    updated = true;
                }

                if (existingPlan.AllowedModelTier != planDef.AllowedModelTier)
                {
                    existingPlan.AllowedModelTier = planDef.AllowedModelTier;
                    updated = true;
                }

                // Upsert features: remove old ones and recreate
                var existingFeatures = await featureRepo.GetListAsync(
                    f => f.PlanId == existingPlan.Id, cancellationToken);

                var existingFeatureSet = existingFeatures.Select(f => f.Feature).ToHashSet();
                var desiredFeatureSet = planDef.Features.ToHashSet();

                if (!existingFeatureSet.SetEquals(desiredFeatureSet))
                {
                    // Remove features no longer in the plan
                    foreach (var oldFeature in existingFeatures.Where(f => !desiredFeatureSet.Contains(f.Feature)))
                    {
                        featureRepo.Delete(oldFeature);
                    }

                    // Add new features not yet in the plan
                    foreach (var feature in desiredFeatureSet.Except(existingFeatureSet))
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
