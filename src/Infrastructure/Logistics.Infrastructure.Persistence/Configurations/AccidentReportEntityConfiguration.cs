using Logistics.Domain.Entities.Safety;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class AccidentReportEntityConfiguration : IEntityTypeConfiguration<AccidentReport>
{
    public void Configure(EntityTypeBuilder<AccidentReport> builder)
    {
        builder.ToTable("AccidentReports");

        builder.HasIndex(i => new { i.DriverId, i.AccidentDateTime });
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.Severity);

        builder.HasOne(i => i.Driver)
            .WithMany()
            .HasForeignKey(i => i.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Truck)
            .WithMany()
            .HasForeignKey(i => i.TruckId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Trip)
            .WithMany()
            .HasForeignKey(i => i.TripId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.ReviewedBy)
            .WithMany()
            .HasForeignKey(i => i.ReviewedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Witnesses)
            .WithOne(i => i.AccidentReport)
            .HasForeignKey(i => i.AccidentReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.ThirdParties)
            .WithOne(i => i.AccidentReport)
            .HasForeignKey(i => i.AccidentReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(i => i.Address).HasMaxLength(500);
        builder.Property(i => i.Description).HasMaxLength(4000);
        builder.Property(i => i.WeatherConditions).HasMaxLength(200);
        builder.Property(i => i.RoadConditions).HasMaxLength(200);
        builder.Property(i => i.InjuryDescription).HasMaxLength(2000);
        builder.Property(i => i.VehicleDamageDescription).HasMaxLength(2000);
        builder.Property(i => i.PoliceReportNumber).HasMaxLength(100);
        builder.Property(i => i.PoliceOfficerName).HasMaxLength(200);
        builder.Property(i => i.PoliceOfficerBadge).HasMaxLength(50);
        builder.Property(i => i.PoliceDepartment).HasMaxLength(200);
        builder.Property(i => i.InsuranceClaimNumber).HasMaxLength(100);
        builder.Property(i => i.DriverStatement).HasMaxLength(4000);
        builder.Property(i => i.ReviewNotes).HasMaxLength(2000);
        builder.Property(i => i.EstimatedDamageCost).HasPrecision(18, 2);
    }
}
