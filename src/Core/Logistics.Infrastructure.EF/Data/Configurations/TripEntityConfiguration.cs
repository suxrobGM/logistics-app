using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.EF.Data.Configurations;

public class TripEntityConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.ToTable("Trips");
        
        builder.Property(i => i.Number)
            .UseIdentityAlwaysColumn()
            .IsRequired()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        
        builder.HasIndex(i => i.Number)
            .IsUnique();
        
        builder.HasMany(i => i.Stops)
            .WithOne(i => i.Trip)
            .HasForeignKey(i => i.TripId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
