using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.EF.Data.Configurations;

public class TruckEntityConfiguration : IEntityTypeConfiguration<Truck>
{
    public void Configure(EntityTypeBuilder<Truck> builder)
    {
        builder.ToTable("Trucks");

        builder.HasMany(i => i.Drivers)
            .WithOne(i => i.Truck)
            .HasForeignKey(i => i.TruckId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(i => i.Loads)
            .WithOne(i => i.AssignedTruck)
            .HasForeignKey(i => i.AssignedTruckId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}