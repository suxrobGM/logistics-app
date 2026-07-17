using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class TruckJurisdictionMileageEntityConfiguration : IEntityTypeConfiguration<TruckJurisdictionMileage>
{
    public void Configure(EntityTypeBuilder<TruckJurisdictionMileage> builder)
    {
        builder.ToTable("truck_jurisdiction_mileage");

        builder.HasOne(i => i.Truck)
            .WithMany()
            .HasForeignKey(i => i.TruckId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ComplexProperty(i => i.Jurisdiction, j =>
        {
            j.Property(p => p.CountryCode).HasMaxLength(2).IsRequired();
            j.Property(p => p.Region).HasMaxLength(10);
        });

        builder.Property(i => i.Miles).HasPrecision(12, 2);

        // Row identity (truck, date, jurisdiction) is enforced by TruckLocationRecorder's
        // upsert — complex-type members can't participate in a unique index.
        builder.HasIndex(i => new { i.TruckId, i.Date });
        builder.HasIndex(i => i.Date);
    }
}
