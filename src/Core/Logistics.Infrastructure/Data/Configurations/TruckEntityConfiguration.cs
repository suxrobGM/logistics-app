using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Data.Configurations;

public class TruckEntityConfiguration : IEntityTypeConfiguration<Truck>
{
    public void Configure(EntityTypeBuilder<Truck> builder)
    {
        builder.ToTable("Trucks");
        
        builder.HasIndex(i => i.Number)
            .IsUnique();

        builder.HasOne(i => i.MainDriver)
            .WithMany()
            .HasForeignKey(i => i.MainDriverId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasOne(i => i.SecondaryDriver)
            .WithMany()
            .HasForeignKey(i => i.SecondaryDriverId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(i => i.Loads)
            .WithOne(i => i.AssignedTruck)
            .HasForeignKey(i => i.AssignedTruckId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
