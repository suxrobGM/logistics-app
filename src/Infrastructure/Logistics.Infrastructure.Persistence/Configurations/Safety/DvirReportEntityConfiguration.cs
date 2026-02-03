using Logistics.Domain.Entities.Safety;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class DvirReportEntityConfiguration : IEntityTypeConfiguration<DvirReport>
{
    public void Configure(EntityTypeBuilder<DvirReport> builder)
    {
        builder.ToTable("dvir_reports");

        builder.HasIndex(i => new { i.TruckId, i.InspectionDate });
        builder.HasIndex(i => new { i.DriverId, i.InspectionDate });
        builder.HasIndex(i => i.Status);

        builder.HasOne(i => i.Truck)
            .WithMany()
            .HasForeignKey(i => i.TruckId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Driver)
            .WithMany()
            .HasForeignKey(i => i.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.ReviewedBy)
            .WithMany()
            .HasForeignKey(i => i.ReviewedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Trip)
            .WithMany()
            .HasForeignKey(i => i.TripId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(i => i.DriverNotes)
            .HasMaxLength(2000);

        builder.Property(i => i.MechanicNotes)
            .HasMaxLength(2000);

        builder.HasMany(i => i.Defects)
            .WithOne(i => i.DvirReport)
            .HasForeignKey(i => i.DvirReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
