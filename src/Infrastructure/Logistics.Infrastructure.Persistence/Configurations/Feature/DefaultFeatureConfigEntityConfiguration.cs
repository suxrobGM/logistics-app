using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class DefaultFeatureConfigEntityConfiguration : IEntityTypeConfiguration<DefaultFeatureConfig>
{
    public void Configure(EntityTypeBuilder<DefaultFeatureConfig> builder)
    {
        builder.ToTable("default_feature_configs");

        // Unique constraint: one config per feature
        builder.HasIndex(e => e.Feature)
            .IsUnique();
    }
}
