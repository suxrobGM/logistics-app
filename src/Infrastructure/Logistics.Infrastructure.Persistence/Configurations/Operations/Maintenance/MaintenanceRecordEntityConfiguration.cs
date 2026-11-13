using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class MaintenanceRecordEntityConfiguration : IEntityTypeConfiguration<MaintenanceRecord>
{
    public void Configure(EntityTypeBuilder<MaintenanceRecord> builder)
    {
        builder.ToTable("maintenance_records");

        builder.HasIndex(i => new { i.TruckId, i.ServiceDate });
        builder.HasIndex(i => i.MaintenanceType);

        builder.HasOne(i => i.Truck)
            .WithMany()
            .HasForeignKey(i => i.TruckId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.PerformedBy)
            .WithMany()
            .HasForeignKey(i => i.PerformedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Parts)
            .WithOne(i => i.MaintenanceRecord)
            .HasForeignKey(i => i.MaintenanceRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(i => i.VendorName)
            .HasMaxLength(200);

        builder.Property(i => i.VendorAddress)
            .HasMaxLength(500);

        builder.Property(i => i.InvoiceNumber)
            .HasMaxLength(100);

        builder.Property(i => i.Description)
            .HasMaxLength(1000);

        builder.Property(i => i.WorkPerformed)
            .HasMaxLength(2000);
    }
}
