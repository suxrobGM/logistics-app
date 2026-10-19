using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class TruckEntityConfiguration : IEntityTypeConfiguration<Truck>
{
    public void Configure(EntityTypeBuilder<Truck> builder)
    {
        builder.ToTable("trucks");

        // Number is unique case-insensitively through the raw-SQL index ux_trucks_number_lower, created in the
        // migration because EF can't model an expression index.
        builder.Property(i => i.Number).HasMaxLength(50);

        builder.Property(i => i.Vin).HasMaxLength(17);
        builder.HasIndex(i => i.Vin)
            .IsUnique()
            .HasFilter("vin IS NOT NULL");

        builder.ComplexProperty(i => i.AdrEquipment, adr =>
        {
            adr.Property(a => a.OrangePlateNumber).HasMaxLength(8);
        });

        builder.HasOne(i => i.MainDriver)
            .WithMany()
            .HasForeignKey(i => i.MainDriverId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.SecondaryDriver)
            .WithMany()
            .HasForeignKey(i => i.SecondaryDriverId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.CurrentTrailer)
            .WithMany()
            .HasForeignKey(i => i.CurrentTrailerId)
            .OnDelete(DeleteBehavior.SetNull);

        // A trailer can be hooked to one truck at a time.
        builder.HasIndex(i => i.CurrentTrailerId)
            .IsUnique()
            .HasFilter("current_trailer_id IS NOT NULL");

        builder.HasMany(i => i.Loads)
            .WithOne(i => i.AssignedTruck)
            .HasForeignKey(i => i.AssignedTruckId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
