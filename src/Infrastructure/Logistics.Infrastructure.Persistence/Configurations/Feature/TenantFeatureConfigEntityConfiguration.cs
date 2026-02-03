using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class TenantFeatureConfigEntityConfiguration : IEntityTypeConfiguration<TenantFeatureConfig>
{
    public void Configure(EntityTypeBuilder<TenantFeatureConfig> builder)
    {
        builder.ToTable("tenant_feature_configs");

        // Unique constraint: one config per feature per tenant
        builder.HasIndex(e => new { e.TenantId, e.Feature })
            .IsUnique();

        builder.HasOne(e => e.Tenant)
            .WithMany(t => t.FeatureConfigs)
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
