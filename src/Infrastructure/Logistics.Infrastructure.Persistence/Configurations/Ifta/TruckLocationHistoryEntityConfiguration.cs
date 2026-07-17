using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class TruckLocationHistoryEntityConfiguration : IEntityTypeConfiguration<TruckLocationHistory>
{
    public void Configure(EntityTypeBuilder<TruckLocationHistory> builder)
    {
        builder.ToTable("truck_location_history");

        builder.HasOne(i => i.Truck)
            .WithMany()
            .HasForeignKey(i => i.TruckId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => new { i.TruckId, i.Timestamp });

        // BRIN suits the append-only, time-ordered access pattern (retention purges, range scans)
        builder.HasIndex(i => i.Timestamp).HasMethod("brin");
    }
}
