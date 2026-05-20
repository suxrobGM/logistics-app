using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class LoadConditionReportEntityConfiguration : IEntityTypeConfiguration<LoadConditionReport>
{
    public void Configure(EntityTypeBuilder<LoadConditionReport> builder)
    {
        builder.ToTable("load_condition_reports");

        builder.Property(v => v.Type)
            .IsRequired();

        builder.Property(v => v.Vin)
            .HasMaxLength(17);

        builder.Property(v => v.VehicleYear);

        builder.Property(v => v.VehicleMake)
            .HasMaxLength(100);

        builder.Property(v => v.VehicleModel)
            .HasMaxLength(100);

        builder.Property(v => v.VehicleBodyClass)
            .HasMaxLength(100);

        builder.Property(v => v.ContainerNumber)
            .HasMaxLength(20);

        builder.Property(v => v.SealNumber)
            .HasMaxLength(50);

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

        builder.HasMany(v => v.Defects)
            .WithOne(d => d.LoadConditionReport)
            .HasForeignKey(d => d.LoadConditionReportId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(v => v.LoadId);
        builder.HasIndex(v => v.Vin);
        builder.HasIndex(v => v.ContainerNumber);
        builder.HasIndex(v => v.InspectedAt);
    }
}
