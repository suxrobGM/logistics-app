using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.EF.Data.Configurations;

public class TripLoadEntityConfiguration : IEntityTypeConfiguration<TripLoad>
{
    public void Configure(EntityTypeBuilder<TripLoad> builder)
    {
        builder.HasKey(tl => new { tl.TripId, tl.LoadId });

        builder.HasOne(tl => tl.Trip)
            .WithMany(t => t.Loads)
            .HasForeignKey(tl => tl.TripId);

        builder.HasOne(tl => tl.Load)
            .WithMany() // no back-collection on Load
            .HasForeignKey(tl => tl.LoadId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}