using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Data.Configurations;

public class TripStopEntityConfiguration : IEntityTypeConfiguration<TripStop>
{
    public void Configure(EntityTypeBuilder<TripStop> builder)
    {
        builder.ToTable("TripStops");

        builder.HasOne(i => i.Trip)
            .WithMany(i => i.Stops)
            .HasForeignKey(i => i.TripId)
            .OnDelete(DeleteBehavior.ClientSetNull);
        
        builder.HasOne(i => i.Load)
            .WithOne(i => i.TripStop)
            .HasForeignKey<Load>(i => i.TripStopId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
