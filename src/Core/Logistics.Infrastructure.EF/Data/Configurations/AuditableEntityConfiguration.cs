using Logistics.Domain.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.EF.Data.Configurations;

internal sealed class AuditableEntityConfiguration : IEntityTypeConfiguration<AuditableEntity>
{
    public void Configure(EntityTypeBuilder<AuditableEntity> builder)
    {
        builder.Property(e => e.CreatedAt)
            .HasColumnName("CreatedAt")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.LastModifiedAt)
            .HasColumnName("LastModifiedAt");

        builder.Property(e => e.CreatedBy)
            .HasColumnName("CreatedBy")
            .HasMaxLength(50);

        builder.Property(e => e.LastModifiedBy)
            .HasColumnName("LastModifiedBy")
            .HasMaxLength(50);
    }
}