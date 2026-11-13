using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class SubscriptionPlanEntityConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("subscription_plans");

        builder.Property(i => i.WeeklyAIBudgetUsd)
            .HasPrecision(10, 2);

        builder.HasIndex(i => i.Tier).IsUnique();

        builder.HasMany(i => i.Subscriptions)
            .WithOne(i => i.Plan)
            .HasForeignKey(i => i.PlanId);
    }
}
