using Logistics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Logistics.Infrastructure.Persistence.Configurations;

internal sealed class PostedTruckEntityConfiguration : IEntityTypeConfiguration<PostedTruck>
{
    public void Configure(EntityTypeBuilder<PostedTruck> builder)
    {
        builder.ToTable("posted_trucks");

        builder.HasIndex(i => new { i.TruckId, i.ProviderType })
            .IsUnique();

        builder.HasIndex(i => i.ExternalPostId);
        builder.HasIndex(i => i.Status);

        builder.Property(i => i.ExternalPostId)
            .HasMaxLength(100);

        builder.Property(i => i.EquipmentType)
            .HasMaxLength(50);

        builder.HasOne(i => i.Truck)
            .WithMany()
            .HasForeignKey(i => i.TruckId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
