using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class VehicleConditionReportEntityConfiguration : IEntityTypeConfiguration<VehicleConditionReport>
{
    public void Configure(EntityTypeBuilder<VehicleConditionReport> builder)
    {
        builder.ToTable("VehicleConditionReports");

        builder.Property(v => v.Vin)
            .IsRequired()
            .HasMaxLength(17);

        builder.Property(v => v.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(v => v.VehicleYear);

        builder.Property(v => v.VehicleMake)
            .HasMaxLength(100);

        builder.Property(v => v.VehicleModel)
            .HasMaxLength(100);

        builder.Property(v => v.VehicleBodyClass)
            .HasMaxLength(100);

        builder.Property(v => v.DamageMarkersJson)
            .HasColumnType("jsonb");

        builder.Property(v => v.Notes)
            .HasMaxLength(2000);

        builder.Property(v => v.InspectorSignature)
            .HasMaxLength(2048);

        builder.Property(v => v.Latitude);
        builder.Property(v => v.Longitude);

        builder.Property(v => v.InspectedAt)
            .IsRequired();

        // Relations
        builder.HasOne(v => v.Load)
            .WithMany()
            .HasForeignKey(v => v.LoadId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.InspectedBy)
            .WithMany()
            .HasForeignKey(v => v.InspectedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(v => v.LoadId);
        builder.HasIndex(v => v.Vin);
        builder.HasIndex(v => v.InspectedAt);
    }
}
