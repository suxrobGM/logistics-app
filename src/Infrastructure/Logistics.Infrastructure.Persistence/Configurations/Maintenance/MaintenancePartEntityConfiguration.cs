using Logistics.Domain.Entities.Maintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class MaintenancePartEntityConfiguration : IEntityTypeConfiguration<MaintenancePart>
{
    public void Configure(EntityTypeBuilder<MaintenancePart> builder)
    {
        builder.ToTable("MaintenanceParts");

        builder.HasIndex(i => i.MaintenanceRecordId);

        builder.Property(i => i.PartName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.PartNumber)
            .HasMaxLength(100);

        builder.Property(i => i.UnitCost).HasPrecision(18, 2);
        builder.Property(i => i.TotalCost).HasPrecision(18, 2);
    }
}
