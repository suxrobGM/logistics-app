using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class ProductLicenseHeartbeatEntityConfiguration : IEntityTypeConfiguration<ProductLicenseHeartbeat>
{
    public void Configure(EntityTypeBuilder<ProductLicenseHeartbeat> builder)
    {
        builder.ToTable("product_license_heartbeats");

        builder.Property(x => x.Hostname).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Version).HasMaxLength(64).IsRequired();
        builder.Property(x => x.KeyId).HasMaxLength(64);
        builder.Property(x => x.Licensee).HasMaxLength(256);

        builder.HasIndex(x => x.InstanceId).IsUnique();
        builder.HasIndex(x => x.LastSeenAt);
    }
}
