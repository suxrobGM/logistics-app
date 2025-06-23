using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.EF.Data.Configurations;

public class TripStopEntityConfiguration : IEntityTypeConfiguration<TripStop>
{
    public void Configure(EntityTypeBuilder<TripStop> builder)
    {
        builder.ToTable("TripStops");
        builder.HasKey(i => new { i.TripId, i.Order });

        builder.HasOne(i => i.Trip)
            .WithMany()
            .HasForeignKey(i => i.TripId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
