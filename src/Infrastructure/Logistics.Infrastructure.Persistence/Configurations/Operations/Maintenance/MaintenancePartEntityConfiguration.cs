using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class MaintenancePartEntityConfiguration : IEntityTypeConfiguration<MaintenancePart>
{
    public void Configure(EntityTypeBuilder<MaintenancePart> builder)
    {
        builder.ToTable("maintenance_parts");

        builder.HasIndex(i => i.MaintenanceRecordId);

        builder.Property(i => i.PartName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.PartNumber)
            .HasMaxLength(100);
    }
}
