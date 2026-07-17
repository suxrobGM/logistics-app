using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class FuelCardEntityConfiguration : IEntityTypeConfiguration<FuelCard>
{
    public void Configure(EntityTypeBuilder<FuelCard> builder)
    {
        builder.ToTable("fuel_cards");

        builder.HasIndex(i => new { i.ProviderType, i.ExternalCardId })
            .IsUnique();

        builder.Property(i => i.ExternalCardId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.UnitNumber)
            .HasMaxLength(50);

        builder.HasOne(i => i.Truck)
            .WithMany()
            .HasForeignKey(i => i.TruckId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
