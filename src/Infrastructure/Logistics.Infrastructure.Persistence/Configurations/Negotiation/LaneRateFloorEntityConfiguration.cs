using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class LaneRateFloorEntityConfiguration : IEntityTypeConfiguration<LaneRateFloor>
{
    public void Configure(EntityTypeBuilder<LaneRateFloor> builder)
    {
        builder.ToTable("lane_rate_floors");

        // A null state is the "any state" wildcard and must collide with another wildcard row,
        // so the index has to treat NULLs as equal (PostgreSQL 15+).
        builder.HasIndex(f => new
            {
                f.OriginCountry, f.OriginState, f.DestinationCountry, f.DestinationState
            })
            .IsUnique()
            .AreNullsDistinct(false);

        builder.Property(f => f.OriginCountry)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(f => f.OriginState)
            .HasMaxLength(3);

        builder.Property(f => f.DestinationCountry)
            .HasMaxLength(2)
            .IsRequired();

        builder.Property(f => f.DestinationState)
            .HasMaxLength(3);

        builder.Property(f => f.MinRatePerMile)
            .HasPrecision(18, 2);

        builder.Property(f => f.Notes)
            .HasMaxLength(2000);

        builder.ComplexProperty(f => f.MinTotalRate, money =>
        {
            money.Property(m => m.Amount).HasPrecision(18, 2);
            money.Property(m => m.Currency).HasMaxLength(3);
        });
    }
}
