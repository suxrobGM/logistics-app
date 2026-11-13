using Logistics.Application.Abstractions.AI;
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
        // DVIR is a federal requirement; the broader Safety suite stays Professional
        TenantFeature.Dvir,
        TenantFeature.AgenticDispatch,
        TenantFeature.AICopilot,
        TenantFeature.McpServer,
        TenantFeature.VehicleTransport,
        TenantFeature.CustomerUpdates
    ];

    private static readonly TenantFeature[] ProfessionalFeatures =
    [
        ..StarterFeatures,
        TenantFeature.LoadBoard,
        TenantFeature.Payroll,
        TenantFeature.Timesheets,
        TenantFeature.Safety,
        TenantFeature.Maintenance,
        TenantFeature.Accounting,
        TenantFeature.FuelCards,
        TenantFeature.Ifta,
        TenantFeature.IntermodalContainers,
        TenantFeature.AIRateNegotiation,
        TenantFeature.AITripAlerts
    ];

    private static readonly TenantFeature[] EnterpriseFeatures = Enum.GetValues<TenantFeature>();

    /// <summary>Weekly AI budgets are in USD of model cost. Every run overwrites these three plans.</summary>
    private static readonly PlanDefinition[] Plans =
    [
        new("Starter", "Essential tools for small fleets getting started",
            Price: 29m, PerTruckPrice: 12m, MaxTrucks: 10, PlanTier.Starter,
            WeeklyAIBudgetUsd: 5m, StarterFeatures),
        new("Professional", "Advanced features for growing fleets",
            Price: 79m, PerTruckPrice: 9m, MaxTrucks: 30, PlanTier.Professional,
            WeeklyAIBudgetUsd: 25m, ProfessionalFeatures),
        // Soft cap, overage-metered beyond it (docs/ai-dispatch.md); also the no-budget tenant fallback.
        new("Enterprise", "Full platform access for large operations",
            Price: 169m, PerTruckPrice: 6m, MaxTrucks: null, PlanTier.Enterprise,
            WeeklyAIBudgetUsd: AIWeeklyBudget.DefaultUsd, EnterpriseFeatures)
    ];

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();
        var planRepo = context.MasterUnitOfWork.Repository<SubscriptionPlan>();

        foreach (var planDef in Plans)
        {
            var existingPlan = await planRepo.GetAsync(i => i.Name == planDef.Name, cancellationToken);
            if (existingPlan is null)
            {
                await CreatePlanAsync(context, planDef, cancellationToken);
            }
            else
            {
                await UpdatePlanAsync(context, existingPlan, planDef, cancellationToken);
            }
        }

        LogCompleted();
    }

    private async Task CreatePlanAsync(SeederContext context, PlanDefinition planDef, CancellationToken cancellationToken)
    {
        var featureRepo = context.MasterUnitOfWork.Repository<PlanFeature>();
        var newPlan = new SubscriptionPlan
        {
            Name = planDef.Name,
            Description = planDef.Description,
            Price = new() { Amount = planDef.Price, Currency = "USD" },
            PerTruckPrice = new() { Amount = planDef.PerTruckPrice, Currency = "USD" },
            MaxTrucks = planDef.MaxTrucks,
            Tier = planDef.Tier,
            WeeklyAIBudgetUsd = planDef.WeeklyAIBudgetUsd
        };

        await context.MasterUnitOfWork.Repository<SubscriptionPlan>().AddAsync(newPlan, cancellationToken);
        await context.MasterUnitOfWork.SaveChangesAsync(cancellationToken);

        var distinctFeatures = planDef.Features.Distinct().ToList();
        foreach (var feature in distinctFeatures)
        {
            await featureRepo.AddAsync(new PlanFeature
            {
                PlanId = newPlan.Id,
                Feature = feature
            }, cancellationToken);
        }

        await context.MasterUnitOfWork.SaveChangesAsync(cancellationToken);
        Logger.LogInformation("Created subscription plan '{PlanName}' with {FeatureCount} features",
            planDef.Name, distinctFeatures.Count);
    }

    /// <summary>Copies the definition onto the stored plan; existing prices keep their currency.</summary>
    private async Task UpdatePlanAsync(
        SeederContext context,
        SubscriptionPlan existingPlan,
        PlanDefinition planDef,
        CancellationToken cancellationToken)
    {
        existingPlan.Description = planDef.Description;
        existingPlan.Price = new() { Amount = planDef.Price, Currency = existingPlan.Price.Currency };
        existingPlan.PerTruckPrice = new() { Amount = planDef.PerTruckPrice, Currency = existingPlan.PerTruckPrice.Currency };
        existingPlan.MaxTrucks = planDef.MaxTrucks;
        existingPlan.Tier = planDef.Tier;
        existingPlan.WeeklyAIBudgetUsd = planDef.WeeklyAIBudgetUsd;
        await SyncPlanFeaturesAsync(context, existingPlan, planDef, cancellationToken);

        await context.MasterUnitOfWork.SaveChangesAsync(cancellationToken);
        Logger.LogInformation("Synced subscription plan '{PlanName}'", planDef.Name);
    }

    /// <summary>Removes features the plan no longer has and adds the missing ones.</summary>
    private static async Task SyncPlanFeaturesAsync(
        SeederContext context,
        SubscriptionPlan existingPlan,
        PlanDefinition planDef,
        CancellationToken cancellationToken)
    {
        var featureRepo = context.MasterUnitOfWork.Repository<PlanFeature>();
        var existingFeatures = await featureRepo.GetListAsync(f => f.PlanId == existingPlan.Id, cancellationToken);

        var existingFeatureSet = existingFeatures.Select(f => f.Feature).ToHashSet();
        var desiredFeatureSet = planDef.Features.ToHashSet();

        foreach (var oldFeature in existingFeatures.Where(f => !desiredFeatureSet.Contains(f.Feature)))
        {
            featureRepo.Delete(oldFeature);
        }

        foreach (var feature in desiredFeatureSet.Except(existingFeatureSet))
        {
            await featureRepo.AddAsync(new PlanFeature
            {
                PlanId = existingPlan.Id,
                Feature = feature
            }, cancellationToken);
        }
    }

    private record PlanDefinition(
        string Name,
        string Description,
        decimal Price,
        decimal PerTruckPrice,
        int? MaxTrucks,
        PlanTier Tier,
        decimal WeeklyAIBudgetUsd,
        TenantFeature[] Features);
}
