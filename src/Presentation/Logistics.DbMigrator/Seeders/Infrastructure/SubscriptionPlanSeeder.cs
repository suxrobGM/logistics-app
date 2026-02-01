using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;

namespace Logistics.DbMigrator.Seeders.Infrastructure;

/// <summary>
/// Seeds subscription plans with upsert logic.
/// </summary>
internal class SubscriptionPlanSeeder(ILogger<SubscriptionPlanSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(SubscriptionPlanSeeder);
    public override SeederType Type => SeederType.Infrastructure;
    public override int Order => 30;

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();
        var repo = context.MasterUnitOfWork.Repository<SubscriptionPlan>();

        var standardPlan = new SubscriptionPlan
        {
            Name = "Standard",
            Description = "Standard monthly subscription plan charging per employee",
            Price = 30
        };

        var existingPlan = await repo.GetAsync(i => i.Name == standardPlan.Name, cancellationToken);

        if (existingPlan is null)
        {
            await repo.AddAsync(standardPlan, cancellationToken);
            await context.MasterUnitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Created subscription plan '{PlanName}'", standardPlan.Name);
        }
        else
        {
            // Upsert: update price/description if changed
            var updated = false;
            if (existingPlan.Price != standardPlan.Price)
            {
                existingPlan.Price = standardPlan.Price;
                updated = true;
            }

            if (existingPlan.Description != standardPlan.Description)
            {
                existingPlan.Description = standardPlan.Description;
                updated = true;
            }

            if (updated)
            {
                await context.MasterUnitOfWork.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Updated subscription plan '{PlanName}'", standardPlan.Name);
            }
            else
            {
                logger.LogInformation("Subscription plan '{PlanName}' already up to date", standardPlan.Name);
            }
        }

        LogCompleted();
    }
}
