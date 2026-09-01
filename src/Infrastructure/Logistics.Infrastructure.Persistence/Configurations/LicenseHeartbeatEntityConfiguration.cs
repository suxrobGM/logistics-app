using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class LicenseHeartbeatEntityConfiguration : IEntityTypeConfiguration<LicenseHeartbeat>
{
    public void Configure(EntityTypeBuilder<LicenseHeartbeat> builder)
    {
        builder.ToTable("license_heartbeats");

        builder.Property(x => x.Hostname).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Version).HasMaxLength(64).IsRequired();
        builder.Property(x => x.KeyId).HasMaxLength(64);
        builder.Property(x => x.Licensee).HasMaxLength(256);

        builder.HasIndex(x => x.InstanceId).IsUnique();
        builder.HasIndex(x => x.LastSeenAt);
    }
}
