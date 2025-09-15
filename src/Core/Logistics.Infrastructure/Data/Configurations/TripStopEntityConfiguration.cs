using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Data.Configurations;

internal sealed class TripStopEntityConfiguration : IEntityTypeConfiguration<TripStop>
{
    public void Configure(EntityTypeBuilder<TripStop> builder)
    {
        builder.ToTable("TripStops");

        builder.HasOne(i => i.Trip)
            .WithMany(i => i.Stops)
            .HasForeignKey(i => i.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ts => ts.Load)
            .WithMany(l => l.TripStops)
            .HasForeignKey(ts => ts.LoadId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
