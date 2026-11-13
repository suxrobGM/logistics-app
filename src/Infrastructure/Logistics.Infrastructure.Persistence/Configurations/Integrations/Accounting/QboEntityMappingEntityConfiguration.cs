using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class QboEntityMappingEntityConfiguration : IEntityTypeConfiguration<QboEntityMapping>
{
    public void Configure(EntityTypeBuilder<QboEntityMapping> builder)
    {
        builder.ToTable("qbo_entity_mappings");

        builder.HasIndex(i => new { i.LocalEntityType, i.LocalId })
            .IsUnique();

        builder.HasIndex(i => i.SyncStatus);

        builder.Property(i => i.QboId)
            .HasMaxLength(100);

        builder.Property(i => i.QboSyncToken)
            .HasMaxLength(50);

        builder.Property(i => i.LastSyncedHash)
            .HasMaxLength(128);

        builder.Property(i => i.LastError)
            .HasMaxLength(2000);
    }
}
