using Logistics.Domain.Entities.Maintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class MaintenanceRecordEntityConfiguration : IEntityTypeConfiguration<MaintenanceRecord>
{
    public void Configure(EntityTypeBuilder<MaintenanceRecord> builder)
    {
        builder.ToTable("MaintenanceRecords");

        builder.HasIndex(i => new { i.TruckId, i.ServiceDate });
        builder.HasIndex(i => i.MaintenanceType);

        builder.HasOne(i => i.Truck)
            .WithMany()
            .HasForeignKey(i => i.TruckId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.MaintenanceSchedule)
            .WithMany()
            .HasForeignKey(i => i.MaintenanceScheduleId)
            .OnDelete(DeleteBehavior.SetNull);

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

        builder.Property(i => i.LaborCost).HasPrecision(18, 2);
        builder.Property(i => i.PartsCost).HasPrecision(18, 2);
        builder.Property(i => i.TotalCost).HasPrecision(18, 2);
    }
}
