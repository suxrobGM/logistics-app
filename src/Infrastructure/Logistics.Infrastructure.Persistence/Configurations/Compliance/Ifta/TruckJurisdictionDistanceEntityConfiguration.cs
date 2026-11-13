using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class TruckJurisdictionDistanceEntityConfiguration : IEntityTypeConfiguration<TruckJurisdictionDistance>
{
    public void Configure(EntityTypeBuilder<TruckJurisdictionDistance> builder)
    {
        builder.ToTable("truck_jurisdiction_distances");

        builder.HasOne(i => i.Truck)
            .WithMany()
            .HasForeignKey(i => i.TruckId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ComplexProperty(i => i.Jurisdiction, j =>
        {
            j.Property(p => p.CountryCode).HasMaxLength(2).IsRequired();
            j.Property(p => p.Region).HasMaxLength(10);
        });

        builder.Property(i => i.Meters).HasPrecision(14, 2);

        // The unique index for the daily upsert is created with raw SQL in the migration, since EF cannot index complex-type members.
        builder.HasIndex(i => new { i.TruckId, i.Date });
        builder.HasIndex(i => i.Date);
    }
}
