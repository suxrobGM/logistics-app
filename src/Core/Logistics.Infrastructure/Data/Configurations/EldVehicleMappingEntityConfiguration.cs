using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Data.Configurations;

internal sealed class EldVehicleMappingEntityConfiguration : IEntityTypeConfiguration<EldVehicleMapping>
{
    public void Configure(EntityTypeBuilder<EldVehicleMapping> builder)
    {
        builder.ToTable("EldVehicleMappings");

        builder.HasIndex(i => new { i.ProviderType, i.ExternalVehicleId })
            .IsUnique();

        builder.HasIndex(i => new { i.ProviderType, i.TruckId })
            .IsUnique();

        builder.HasOne(i => i.Truck)
            .WithMany()
            .HasForeignKey(i => i.TruckId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(i => i.ExternalVehicleId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.ExternalVehicleName)
            .HasMaxLength(200);
    }
}
