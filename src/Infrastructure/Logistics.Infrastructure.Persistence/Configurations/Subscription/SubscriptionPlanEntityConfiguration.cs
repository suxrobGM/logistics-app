using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class SubscriptionPlanEntityConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("subscription_plans");

        builder.ComplexProperty(i => i.Price, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2);
            money.Property(m => m.Currency).HasMaxLength(3);
        });

        builder.ComplexProperty(i => i.PerTruckPrice, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2);
            money.Property(m => m.Currency).HasMaxLength(3);
        });

        builder.Property(i => i.AnnualDiscountPercent).HasPrecision(5, 2);

        builder.HasIndex(i => i.Tier).IsUnique();

        builder.HasMany(i => i.Subscriptions)
            .WithOne(i => i.Plan)
            .HasForeignKey(i => i.PlanId);
    }
}
